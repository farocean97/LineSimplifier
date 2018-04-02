using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LineSimplifier {
	public class DPHull:LineSimplifier {
		public enum eBuildStep {
			OutputVertex = 0,
			DP,
			Build,
			ReturnKey,
            UpdateHull
            
		}

		private class SBuildStep {
            public PathHullStack LH { get; set; }
            public PathHullStack RH { get; set; }
			public int iB { get; set; }
			public int iE { get; set; }
			public eBuildStep eBuild { get; set; }
			public SBuildStep(PathHullStack lh, PathHullStack rh, int i_b, int i_e, eBuildStep sb) {
                this.LH = lh;
                this.RH = rh;
				this.iB = i_b;
				this.iE = i_e;
				this.eBuild = sb;
			}
		}

		private Stack<SBuildStep> m_stack;

		private PathHullStack m_RHull;

		private PathHullStack m_LHull;

		private int m_PHTag;

        private Stack<int> m_keys;


		public DPHull(IList<Point> pts, bool bnorm):base(pts, bnorm) {
			m_stack = new Stack<SBuildStep> { };
            m_keys = new Stack<int> { };
		}

		private void AddBuildStep(PathHullStack lh, PathHullStack rh, int ib, int ie, eBuildStep eb) {
			m_stack.Push(new SBuildStep(lh,rh,ib, ie, eb));
		}

		private void OutputVertex() {
            //Debug.Assert(m_stack.Count > 0);
            //Debug.Assert(m_stack.First().eBuild == eBuildStep.ReturnKey);
            //GetKeys.Add(m_stack.First().iB);
            //m_stack.Pop();
			Debug.Assert(m_stack.Count > 0);
			Debug.Assert(m_stack.First().eBuild == eBuildStep.OutputVertex);
            Debug.Assert(m_keys.Count > 0);
            GetKeys.Add(m_keys.Pop());
			m_stack.Pop();
		}

        private void ReturnKey() {
            Debug.Assert(m_stack.Count > 0);
            Debug.Assert(m_stack.First().eBuild == eBuildStep.ReturnKey);
            //m_keys.Push(m_stack.First().iB);
            GetKeys.Add(m_stack.First().iB);
            m_stack.Pop();
        }

        private void UpdateHull() {
            Debug.Assert(m_stack.Count > 0);
            Debug.Assert(m_stack.First().eBuild == eBuildStep.UpdateHull);
            //m_keys.Push(m_stack.First().iB);
            m_LHull = m_stack.First().LH;
            m_RHull = m_stack.First().RH;
            m_PHTag = m_stack.First().iB;
            m_stack.Pop();
        }

		private void Build() {
			Debug.Assert(m_stack.Count() > 0);
			Debug.Assert(m_stack.First().eBuild == eBuildStep.Build);
			int ib = m_stack.First().iB;
			int ie = m_stack.First().iE;
			m_stack.Pop();
			m_PHTag =  (ib + ie) / 2;
			m_LHull = new PathHullStack(Points,ib, m_PHTag, true);
			m_RHull = new PathHullStack(Points, m_PHTag, ie, false);
		}

		private void DP() {
			double rd=0, ld=0, lensq=0;
			int re=0, le=0;
			Debug.Assert(m_stack.Count() > 0);
			Debug.Assert(m_stack.First().eBuild == eBuildStep.DP);
			int ib = m_stack.First().iB;
			int ie = m_stack.First().iE;
			m_stack.Pop();
			SHomog line = new SHomog(Points[ib], Points[ie]);
			lensq = Points[ib].SqrDist(Points[ie]);

            if ((ie - ib) < 2) {
                AddBuildStep(null, null, ie, ie, eBuildStep.ReturnKey);
                return;
            }
			this.m_LHull.Find_Extreme(line, ref le, ref ld);
			this.m_RHull.Find_Extreme(line, ref re, ref rd);
 			if (ld > rd) {
				if (ld * ld > Tol * lensq) {
                    PathHullStack lh = m_LHull.Copy();
                    PathHullStack rh = m_RHull.Copy();
					lh.Split(le);
                    //AddBuildStep(null, null, 0, 0, eBuildStep.OutputVertex);
                    AddBuildStep(null, null, le, ie, eBuildStep.DP);
                    AddBuildStep(lh, rh, m_PHTag, 0, eBuildStep.UpdateHull);
					AddBuildStep(null, null, ib, le, eBuildStep.DP);
					AddBuildStep(null, null, ib, le, eBuildStep.Build);
                    //AddBuildStep(null, null, le, ie, eBuildStep.DP);
					return;
				} else {
					AddBuildStep(null, null, ie, ie, eBuildStep.ReturnKey);
					return;
				}
			} else {
				if (rd * rd > Tol * lensq) {
					if (m_PHTag != re) {
						m_RHull.Split(re);
					}
                    AddBuildStep(null, null, re, ie, eBuildStep.DP);
                    AddBuildStep(null, null,re, ie, eBuildStep.Build);
                   // AddBuildStep(null, null,0, 0, eBuildStep.OutputVertex);
                    AddBuildStep(null, null,ib, re, eBuildStep.DP);
					if (m_PHTag == re) {
						AddBuildStep(null, null,ib, re, eBuildStep.Build);
					}
					return;
				} else {
					AddBuildStep(null, null,ie, ie, eBuildStep.ReturnKey);
				} 
			}


			//if ((ie - ib) < 8) {
			//    rd = 0;
			//    for (int i = ib + 1; i < ie; i++) {
			//        ld = Math.Abs(line.DotProduct_2CH(Points[i]));
			//        if (ld > rd) {
			//            rd = ld;
			//            re = i;
			//        }
			//    }
			//    if (rd * rd > Tol * lensq) {
			//        AddBuildStep(re, ie, eBuildStep.DP);
			//        AddBuildStep(ib, re, eBuildStep.OutputVertex);
			//        AddBuildStep(ib, re, eBuildStep.DP);
			//        return;

			//    } else {
			//        AddBuildStep(ie, ie, eBuildStep.ReturnKey);
			//        return;
			//    }
			//} else {

			//}



		}

		override public void ComputeKeys() {

			double eps = 0.001;
			double eps2 = eps * eps;
			int ib = 0;
			int ie = this.NumOfPoints-1;
			SHomog line = new SHomog(Points[ib], Points[ie]);
			double lensq = Points[ib].SqrDist(Points[ie]);
			if (lensq < eps2) {
				throw new Exception("start and end points are too close");
			}

			//ie--;

			AddBuildStep(null, null,ib, ie, eBuildStep.Build);
			Build();
			//AddBuildStep(ib, ib, eBuildStep.OutputVertex);
            AddBuildStep(null, null, ib, ib, eBuildStep.ReturnKey);
            ReturnKey();
            //AddBuildStep(null, null, 0, 0, eBuildStep.OutputVertex);
			//OutputVertex();
            //AddBuildStep(null, null, 0, 0, eBuildStep.OutputVertex);

			//AddBuildStep(ib, ib, eBuildStep.OutputVertex);
            AddBuildStep(null, null, ib, ie, eBuildStep.DP);

			while (m_stack.Count() > 0) {
				switch (m_stack.First().eBuild) {
					case eBuildStep.DP:
						DP();
						break;
					case eBuildStep.Build:
						Build();
						break;
					case eBuildStep.ReturnKey:
						ReturnKey();
						break;
					case eBuildStep.OutputVertex:
                        //OutputVertex();
                        Debug.Assert(false);
						break;
                    case eBuildStep.UpdateHull:
                        UpdateHull();
                        break;
					default:
						Debug.Assert(false);
						break;
				}
			}

		}



	}
}
