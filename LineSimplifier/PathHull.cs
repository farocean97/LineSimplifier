using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace LineSimplifier {
   public  class PathHull {
       public enum eStackOp {
           Push = 0,
           Top,
           Bot
       }

       public IList<Point> Pts {get;set;}
       public int[] Elt { get; set; }
       public int[] HElt { get; set; }
       public eStackOp[] Op { get; set; }
       public int HMax { get; set; }
       public int i_Begin { get; set; }
       public int i_End { get; set; }
       public int Top { get; set; }
       public int Bot { get; set; }
       public int Hp { get; set; }
       public bool isLeft { get; set; }

       public PathHull() { }

       public PathHull(IList<Point> pts, int i_b, int i_e, bool isleft) {
           Pts = pts;
           Debug.Assert(i_e >= i_b);
           Debug.Assert(i_b >= 0);
           Debug.Assert(i_e < pts.Count());
           i_Begin = i_b;
           i_End = i_e;
           HMax = Math.Max(4,i_e - i_b+1);
           Elt = new int[2 * HMax];
           HElt = new int[3 * HMax];
           Op = new eStackOp[3 * HMax];
           isLeft = isleft;
           HBuild(isLeft);
       }



       private bool LeftOf(int a, int b, int c) {
           return (Pts[a].X - Pts[c].X) * (Pts[b].Y - Pts[c].Y) >= (Pts[b].X - Pts[c].X) * (Pts[a].Y - Pts[c].Y);
       }


       private void HPush(int ie) {
           Elt[++Top] = ie;
           Elt[--Bot] = ie;
           HElt[++Hp] = ie;
           Op[Hp] = eStackOp.Push;
       }

       private void HPopTop() {
           HElt[++Hp] = Elt[Top];
           Op[Hp] = eStackOp.Top;
           Elt[Top] = -1;
           Top--;
       }

       private void HPopBot() {
           HElt[++Hp] = Elt[Bot];
           Op[Hp] = eStackOp.Bot;
           Elt[Bot] = -1;
           Bot++;
       }

       private void HInit(int ib, int ie) {
           Elt[HMax] = ib;
           Top = HMax + 1;
           Elt[Top] = ie;
           Bot = HMax - 1;
           Elt[Bot] = ie;
           Hp = 0;
           HElt[Hp] = ie;
           Op[Hp] = eStackOp.Push;
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
           int tmpe;
           eStackOp tmpo;
           tmpo = Op[Hp];
           tmpe = HElt[Hp];
           while (Hp > 0 && (tmpe != ie || tmpo != eStackOp.Push)) {
               Hp--;
               switch (tmpo) {
                   case eStackOp.Push:
                       Top--;
                       Bot++;
                       break;
                   case eStackOp.Top:
                       Elt[++Top] = tmpe;
                       break;
                   case eStackOp.Bot:
                       Elt[--Bot] = tmpe;
                       break;
                   default:
                       break;
               }
               tmpo = Op[Hp];
               tmpe = HElt[Hp];
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

       public PathHull Copy() {
           PathHull ph = new PathHull(){};
           ph.Pts = this.Pts;
           ph.i_Begin = this.i_Begin;
           ph.i_End = this.i_End;
           ph.HMax = this.HMax;
           ph.Elt = new int[2 * HMax];
           ph.HElt = new int[3 * HMax];
           ph.Op = new eStackOp[3 * HMax];
           this.Elt.CopyTo(ph.Elt,0);
           this.HElt.CopyTo(ph.HElt, 0);
           this.Op.CopyTo(ph.Op, 0);
           ph.Top = this.Top;
           ph.Bot = this.Bot;
           ph.Hp = this.Hp;
           ph.isLeft = this.isLeft;
           return ph;
       }

            

    }
}
