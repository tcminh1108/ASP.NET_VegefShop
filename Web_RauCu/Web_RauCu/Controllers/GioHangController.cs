using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_RauCu.Models;
namespace Web_RauCu.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        dbQLNongsanDataContext data = new dbQLNongsanDataContext();
        //Lay gio hang
        public List<GioHang> Laygiohang()
        {
            List<GioHang> lstGiohang = Session["Giohang"] as List<GioHang>;
            if (lstGiohang == null)
            {
                //Neu gio hang chua ton tai thi khoi tao listGiohang
                lstGiohang = new List<GioHang>();
                Session["Giohang"] = lstGiohang;
            }
            return lstGiohang;
        }
        //Them hang vao gio
        public ActionResult Themgiohang(int Ma_SP, string strURL)
        {
            //Lay ra Session gio hang
            List<GioHang> lstGiohang = Laygiohang();
            //Kiem tra sách này tồn tại trong Session["Giohang"] chưa?
            GioHang sanpham = lstGiohang.Find(n => n.iMa_SP == Ma_SP);
            if (sanpham == null)
            {
                sanpham = new GioHang(Ma_SP);
                lstGiohang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.iSoluong++;
                return Redirect(strURL);
            }
        }
        public ActionResult CapNhatGioHang(int Ma_SP, FormCollection collection)
        {
            //Lay ra Session gio hang
            List<GioHang> lstGiohang = Laygiohang();
            //Kiem tra sách này tồn tại trong Session["Giohang"] chưa?
            GioHang sanpham = lstGiohang.Find(n => n.iMa_SP == Ma_SP);
            sanpham.iSoluong = int.Parse(collection["soluong"].ToString());
            return Redirect("GioHang");
        }
        //Tong so luong
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGiohang = Session["GioHang"] as List<GioHang>;
            if (lstGiohang != null)
            {
                iTongSoLuong = lstGiohang.Sum(n => n.iSoluong);
            }
            return iTongSoLuong;
        }
        //Tinh tong tien
        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGiohang = Session["GioHang"] as List<GioHang>;
            if (lstGiohang != null)
            {
                dTongTien = lstGiohang.Sum(n => n.dThanhtien);
            }
            return dTongTien;
        }
        //Hiển thi giỏ hàng.
        public ActionResult GioHang()
        {
            List<GioHang> lstGiohang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        //Xoa Giohang
        public ActionResult XoaSPgiohang(int Ma_SP)
        {
            //Lay gio hang tu Session
            List<GioHang> lstGiohang = Laygiohang();
            //Kiem tra san pham da co trong Session["Giohang"]
            GioHang sanpham = lstGiohang.SingleOrDefault(n => n.iMa_SP == Ma_SP);
            //Neu ton tai thi cho sua Soluong
            if (sanpham != null)
            {
                lstGiohang.RemoveAll(n => n.iMa_SP == Ma_SP);
                return RedirectToAction("GioHang");
            }
            return RedirectToAction("GioHang");
        }
        //Xóa tất cả giỏ hàng
        public ActionResult Xoagiohang()
        {
            List<GioHang> lstGiohang = Laygiohang();
            lstGiohang.Clear();
            return RedirectToAction("Index", "VegefShop");
        }
        //đặt hàng
        [HttpGet]
        public ActionResult Dathang()
        {
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            if (Session["Giohang"] == null)
            {
                return RedirectToAction("Index", "VegefShop");
            }
            List<GioHang> lstGiohang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        [HttpPost]
        //Xay dung chuc nang Dathang
        public ActionResult DatHang(FormCollection collection)
        {
            //Them Don hang
            DonDatHang ddh = new DonDatHang();
            NguoiDung nd = (NguoiDung)Session["Taikhoan"];
            List<GioHang> gh = Laygiohang();
            ddh.TongTien = (decimal?)TongTien();
            ddh.Ma_ND = nd.Ma_ND;
            ddh.Ngaydat = DateTime.Now;
            ddh.Dathanhtoan = false;
            data.DonDatHangs.InsertOnSubmit(ddh);
            data.SubmitChanges();
            //Them chi tiet don hang            
            foreach (var item in gh)
            {
                CT_DonDatHang ctdh = new CT_DonDatHang();
                ctdh.Ma_DDH = ddh.Ma_DDH;
                ctdh.Ma_SP = item.iMa_SP;
                ctdh.Soluong = item.iSoluong;
                ctdh.Giaban = (decimal)item.dGiaban;
                data.CT_DonDatHangs.InsertOnSubmit(ctdh);
            }
            data.SubmitChanges();
            Session["Giohang"] = null;
            return RedirectToAction("Index", "VegefShop");
        }
        public ActionResult Xacnhandonhang()
        {
            return View();
        }
    }
}