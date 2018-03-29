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
			ReturnKey
		}

		private class SBuildStep {
			public int iB { get; set; }
			public int iE { get; set; }
			public eBuildStep eBuild { get; set; }
			public SBuildStep(int i_b, int i_e, eBuildStep sb) {
				this.iB = i_b;
				this.iE = i_e;
				this.eBuild = sb;
			}
		}

		private Stack<SBuildStep> m_stack;

		private PathHull m_RHull;

		private PathHull m_LHull;

		private int m_PHTag;

        private Stack<int> m_keys;


		public DPHull(IList<Point> pts, bool bnorm):base(pts, bnorm) {
			m_stack = new Stack<SBuildStep> { };
            m_keys = new Stack<int> { };
		}

		private void AddBuildStep(int ib, int ie, eBuildStep eb) {
			m_stack.Push(new SBuildStep(ib, ie, eb));
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
            m_keys.Push(m_stack.First().iB);
            m_stack.Pop();
        }

		private void Build() {
			Debug.Assert(m_stack.Count() > 0);
			Debug.Assert(m_stack.First().eBuild == eBuildStep.Build);
			int ib = m_stack.First().iB;
			int ie = m_stack.First().iE;
			m_stack.Pop();
			m_PHTag =  (ib + ie) / 2;
			m_LHull = new PathHull(Points,ib, m_PHTag, true);
			m_RHull = new PathHull(Points, m_PHTag, ie, false);
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
                AddBuildStep(ie, ie, eBuildStep.ReturnKey);
                return;
            }
			this.m_LHull.Find_Extreme(line, ref le, ref ld);
			this.m_RHull.Find_Extreme(line, ref re, ref rd);
 			if (ld > rd) {
				if (ld * ld > Tol * lensq) {
					m_LHull.Split(le);
                    AddBuildStep(0, 0, eBuildStep.OutputVertex);
					AddBuildStep(ib, le, eBuildStep.DP);
					AddBuildStep(ib, le, eBuildStep.Build);
                    AddBuildStep(le, ie, eBuildStep.DP);
					return;
				} else {
					AddBuildStep(ie, ie, eBuildStep.ReturnKey);
					return;
				}
			} else {
				if (rd * rd > Tol * lensq) {
					if (m_PHTag != re) {
						m_RHull.Split(re);
					}
                    AddBuildStep(re, ie, eBuildStep.DP);
                    AddBuildStep(re, ie, eBuildStep.Build);
                    AddBuildStep(0, 0, eBuildStep.OutputVertex);
                    AddBuildStep(ib, re, eBuildStep.DP);
					if (m_PHTag == re) {
						AddBuildStep(ib, re, eBuildStep.Build);
					}
					return;
				} else {
					AddBuildStep(ie, ie, eBuildStep.ReturnKey);
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

			AddBuildStep(ib, ie, eBuildStep.Build);
			Build();
			//AddBuildStep(ib, ib, eBuildStep.OutputVertex);
            AddBuildStep(ib, ib, eBuildStep.ReturnKey);
            ReturnKey();
            AddBuildStep(0, 0, eBuildStep.OutputVertex);
			OutputVertex();
            AddBuildStep(0, 0, eBuildStep.OutputVertex);
			//AddBuildStep(ib, ib, eBuildStep.OutputVertex);
			AddBuildStep(ib, ie, eBuildStep.DP);

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
                        OutputVertex();
						break;
					default:
						Debug.Assert(false);
						break;
				}
			}

		}



	}
}
