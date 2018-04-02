using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LineSimplifier {
	public class PathHullStack {
	   public enum eOp {
           Push = 0,
           Top,
           Bot
       }

	   private class PHullOp {
		   public eOp Op { get; set;}
		   public int Pt_i { get; set; }

		   public PHullOp(eOp op, int pti) {
			   this.Op = op;
			   this.Pt_i = pti;
		   }
	   }

	   private Stack<PHullOp> StackOp { get; set; }

       public IList<Point> Pts {get;set;}
       public int[] Elt { get; set; }
       //public int[] HElt { get; set; }
       //public eStackOp[] Op { get; set; }
       public int HMax { get; set; }
       public int i_Begin { get; set; }
       public int i_End { get; set; }
       public int Top { get; set; }
       public int Bot { get; set; }
       //public int Hp { get; set; }
       public bool isLeft { get; set; }

       public PathHullStack() { }

       public PathHullStack(IList<Point> pts, int i_b, int i_e, bool isleft) {
           Pts = pts;
           Debug.Assert(i_e >= i_b);
           Debug.Assert(i_b >= 0);
           Debug.Assert(i_e < pts.Count());
           i_Begin = i_b;
           i_End = i_e;
           HMax = Math.Max(4,i_e - i_b+1);
           Elt = new int[2 * HMax];
           //HElt = new int[3 * HMax];
		   this.StackOp = new Stack<PHullOp> { };
           isLeft = isleft;
           HBuild(isLeft);
       }



       private bool LeftOf(int a, int b, int c) {
           return (Pts[a].X - Pts[c].X) * (Pts[b].Y - Pts[c].Y) >= (Pts[b].X - Pts[c].X) * (Pts[a].Y - Pts[c].Y);
       }


       private void HPush(int ie) {
           Elt[++Top] = ie;
           Elt[--Bot] = ie;
		   StackOp.Push(new PHullOp(eOp.Push, ie));
           //HElt[++Hp] = ie;
           //Op[Hp] = eStackOp.Push;
       }

       private void HPopTop() {
		   StackOp.Push(new PHullOp(eOp.Top, Elt[this.Top]));
		   //HElt[++Hp] = Elt[Top];
		   //Op[Hp] = eStackOp.Top;
           Elt[Top] = -1;
           Top--;
       }

       private void HPopBot() {
		   StackOp.Push(new PHullOp(eOp.Bot, Elt[this.Bot]));
           //HElt[++Hp] = Elt[Bot];
           //Op[Hp] = eStackOp.Bot;
           Elt[Bot] = -1;
           Bot++;
       }

       private void HInit(int ib, int ie) {
           Elt[HMax] = ib;
           Top = HMax + 1;
           Elt[Top] = ie;
           Bot = HMax - 1;
           Elt[Bot] = ie;
		   //Hp = 0;
		   //HElt[Hp] = ie;
		   //Op[Hp] = eStackOp.Push;
		   StackOp.Push(new PHullOp(eOp.Push, ie));
       }

       private void HAdd(int ie) {
           Debug.Assert(ie >= i_Begin && ie <= i_End);
           bool topflag, botflag;
           topflag = LeftOf(Elt[Top], Elt[Top - 1], ie);
           botflag = LeftOf(Elt[Bot + 1], Elt[Bot], ie);
           if (topflag || botflag) {
               while (topflag) {
                   HPopTop();
                   topflag = LeftOf(Elt[Top], Elt[Top - 1], ie);
               }

               while (botflag) {
                   HPopBot();
                   botflag = LeftOf(Elt[Bot +1 ], Elt[Bot], ie);
               }
               HPush(ie);
           }
       }

       private void HBuild(bool isleft) {
           if (isleft) {
               HInit(i_End, i_End - 1);
               for (int k = i_End - 2; k >= i_Begin; k--) {
                   HAdd(k);
               }
           } else {
               HInit(i_Begin, i_Begin + 1);
               for (int k = i_Begin + 2; k <= i_End; k++) {
                   HAdd(k);
               }
           }
       }

       public void Split(int ie) {
           if (!(ie > i_Begin && ie < i_End)) {
               throw new Exception(string.Format("In split ie= {0}, i_Begin = {1}, i_End = {2}", ie, i_Begin, i_End));
           }
           //Debug.Assert(ie > i_Begin && ie < i_End);
		   PHullOp PHOp = this.StackOp.First();
		   while (this.StackOp.Count > 0 && (PHOp.Pt_i != ie || PHOp.Op != eOp.Push)) {
			   this.StackOp.Pop();
			   switch (PHOp.Op) {
                   case eOp.Push:
                       Top--;
                       Bot++;
                       break;
                   case eOp.Top:
					   Elt[++Top] = PHOp.Pt_i;
                       break;
                   case eOp.Bot:
					   Elt[--Bot] = PHOp.Pt_i;
                       break;
                   default:
                       break;
               }
			   PHOp = this.StackOp.First();
           }
       }

       public void Find_Extreme(SHomog line, ref int ie, ref double dist) {
           int  mid, lo, m1, brk, m2, hi;
           bool sbase, sbrk;
           double d1, d2;
           if ((Top - Bot) > 6) {
               lo = Bot;
               hi = Top - 1;
               sbase = line.Slope_Sign(Pts[Elt[hi]], Pts[Elt[lo]]);
               do {
                   brk = (lo + hi) / 2;
                   sbrk = line.Slope_Sign(Pts[Elt[brk]], Pts[Elt[brk + 1]]);
                   if (sbase == sbrk) {
                       if (sbase == line.Slope_Sign(Pts[Elt[lo]], Pts[Elt[brk + 1]])) {
                           lo = brk + 1;
                       } else {
                           hi = brk;
                       }
                   }

               } while (sbase == sbrk);

               m1 = brk;
               while (lo < m1) {
                   mid = (lo + m1) / 2;
                   if (sbase == line.Slope_Sign(Pts[Elt[mid]], Pts[Elt[mid + 1]])) {
                       lo = mid + 1;
                   } else {
                       m1 = mid;
                   }
               }

               m2 = brk;
               while (m2 < hi) {
                   mid = (m2 + hi) / 2;
                   if (sbase == line.Slope_Sign(Pts[Elt[mid]], Pts[Elt[mid + 1]])) {
                       hi = mid;
                   } else {
                       m2 = mid + 1;
                   }
               }
               d1 = Math.Abs(line.DotProduct_2CH(Pts[Elt[lo]]));
               d2 = Math.Abs(line.DotProduct_2CH(Pts[Elt[m2]]));
               if (d1 > d2) {
                   dist = d1;
                   ie = Elt[lo];
               } else {
                   dist = d2;
                   ie = Elt[m2];
               }
           } else {
               dist = 0;
               for (mid = Bot; mid < Top; mid++) {
                   d1 = Math.Abs(line.DotProduct_2CH(Pts[Elt[mid]]));
                   if (d1 > dist) {
                       dist = d1;
                       ie = Elt[mid];
                   }
               }

           }
     
       }

       public PathHullStack Copy() {
           PathHullStack ph = new PathHullStack(){};
           ph.Pts = this.Pts;
           ph.i_Begin = this.i_Begin;
           ph.i_End = this.i_End;
           ph.HMax = this.HMax;
		   ph.Top = this.Top;
		   ph.Bot = this.Bot;
		   //ph.Hp = this.Hp;
           ph.Elt = new int[2 * HMax];
		   //ph.HElt = new int[3 * HMax];
		   //ph.Op = new eStackOp[3 * HMax];
		   //for (int i = 0; i <= this.Hp; i++) {
		   //    ph.Op[i] = this.Op[i];
		   //    ph.HElt[i] = this.HElt[i];
		   //}
		   // Clone stackb
		   var arr = new PHullOp[this.StackOp.Count];
		   this.StackOp.CopyTo(arr, 0);
		   Array.Reverse(arr);
		   ph.StackOp = new Stack<PHullOp>(arr);
		   // Clone stacke

		   for (int i = this.Bot; i <= this.Top; i++) {
			   ph.Elt[i] = this.Elt[i];
		   }
		   //this.Elt.CopyTo(ph.Elt, 0);
		   //this.HElt.CopyTo(ph.HElt, 0);
		   //this.Op.CopyTo(ph.Op, 0);
           ph.isLeft = this.isLeft;
           return ph;
       }

	}
}
