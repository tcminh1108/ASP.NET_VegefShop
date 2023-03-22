using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_RauCu.Models
{
    public class GioHang
    {
        dbQLNongsanDataContext data = new dbQLNongsanDataContext();
        public int iMa_SP { set; get; }
        public string sTen_SP { set; get; }
        public Double dGiaban { set; get; }
        public string sHinhanh { set; get; }
        public int iSoluong { set; get; }
        public int iSoluongton { set; get; }
        public Double dThanhtien
        {
            get { return iSoluong * dGiaban; }

        }
        public GioHang(int Ma_SP)
        {
            iMa_SP = Ma_SP;
            SanPham sp = data.SanPhams.Single(n => n.Ma_SP == iMa_SP);
            sTen_SP = sp.Ten_SP;
            sHinhanh = sp.Hinhanh;
            dGiaban = double.Parse(sp.Giaban.ToString());
            iSoluong = 1;
            iSoluongton = int.Parse(sp.Soluongton.ToString());
        }
    }
}