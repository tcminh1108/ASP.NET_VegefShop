using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_RauCu.Models;

namespace Web_RauCu.Controllers
{
    public class NguoiDungController : Controller
    {
        dbQLNongsanDataContext data = new dbQLNongsanDataContext();
        // GET: NguoiDung
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection, NguoiDung nd)
        {
            var hoten = collection["hoten_dk"];
            var taikhoan = collection["taikhoan_dk"];
            var matkhau = collection["matkhau_dk"];
            var nhaplaimatkhau = collection["nhaplaimatkhau_dk"];
            var gmail = collection["email_dk"];
            var diachi = collection["diachi_dk"];
            var dienthoai = collection["sdt_dk"];
            var kttk = data.NguoiDungs.SingleOrDefault(n => n.Taikhoan_ND == taikhoan);
            if (kttk != null)
            {
                ViewBag.Thongbao = "Tài Khoản đã tồn tại!";
            }
            else
            {
                nd.Hoten_ND = hoten;
                nd.Taikhoan_ND = taikhoan;
                nd.Matkhau_ND = matkhau;
                nd.Gmail_ND = gmail;
                nd.Diachi_ND = diachi;
                nd.Dienthoai_ND = dienthoai;
                data.NguoiDungs.InsertOnSubmit(nd);
                data.SubmitChanges();
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            return this.DangKy();
        }
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            var taikhoan = collection["taikhoan_dn"];
            var matkhau = collection["matkhau_dn"];
            var nd = data.NguoiDungs.SingleOrDefault(n => n.Taikhoan_ND == taikhoan && n.Matkhau_ND == matkhau);
            if (nd != null)
            {
                Session["taikhoan"] = nd;
                return RedirectToAction("Index", "VegefSHop");
            }
            else
            {
                ViewBag.Thongbao = "Tài khoản hoặc mật khẩu không hợp lệ!";
                return this.DangNhap();
            }
                    
        }
        [HttpGet]
        public ActionResult ThongTinCaNhan(int Ma_ND)
        {
            NguoiDung nd = data.NguoiDungs.SingleOrDefault(n => n.Ma_ND == Ma_ND);
            return View(nd);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThongTinCaNhan(int Ma_ND, NguoiDung nd, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                nd = data.NguoiDungs.SingleOrDefault(n => n.Ma_ND == Ma_ND);
                var tennd = collection["tennd"];
                var gmail = collection["gmailnd"];
                var diachi = collection["diachind"];
                var sdt = collection["sdtnd"];
                nd.Hoten_ND = tennd;
                nd.Gmail_ND = gmail;
                nd.Diachi_ND = diachi;
                nd.Dienthoai_ND = sdt;
                UpdateModel(nd);
                data.SubmitChanges();
            }
            return RedirectToAction("ThongTinCaNhan", "NguoiDung");
        }
        public ActionResult DonDatHang(int Ma_ND)
        {
            var ListDDH = data.DonDatHangs.OrderByDescending(n => n.Ma_ND == Ma_ND).ToList();
            return View(ListDDH);            
        }
        public ActionResult CT_DDH(int Ma_DDH)
        {
                var ListCT_DDH = data.CT_DonDatHangs.Where(n => n.Ma_DDH == Ma_DDH).ToList();
                return View(ListCT_DDH);
        }
    }
}