using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_RauCu.Models;
using PagedList;
namespace Web_RauCu.Controllers
{
    public class VegefShopController : Controller
    {
        // GET: VegefShop
        dbQLNongsanDataContext data = new dbQLNongsanDataContext();
        private List<SanPham> LaySPMoi(int soluong)
        {
            //Sắp xếp sản phẩm giảm dần theo ngày đăng bán
            return data.SanPhams.OrderByDescending(sp => sp.Ngaydangban).Take(soluong).ToList();
        }
        public ActionResult Index()
        {
            //lấy 8 sản phẩm mới nhất
            var SPMoi = LaySPMoi(9);
            return View(SPMoi);
        }
        public ActionResult Loaisanpham()
        {
            var loaiSP = from lsp in data.LoaiSanPhams select lsp;
            return PartialView(loaiSP);
        }
        public ActionResult Nhacungcap()
        {
            var NhaCC = from ncc in data.NhaCungCaps select ncc;
            return PartialView(NhaCC);
        }
        public ActionResult SPTheoLoai(int Ma_LSP)
        {
            var SP_TheoLoai = from sptl in data.SanPhams where sptl.Ma_LSP == Ma_LSP select sptl;
            return View(SP_TheoLoai);
        }
        public ActionResult SPTheoNCC(int Ma_NCC)
        {
            var SP_TheoNCC = from ncc in data.SanPhams where ncc.Ma_NCC == Ma_NCC  select ncc;
            return View(SP_TheoNCC);
        }
        public ActionResult TimKiem(string tukhoa)
        {
            var listTimKiem = data.SanPhams.Where(n => n.Ten_SP.Contains(tukhoa));
            ViewBag.tukhoa = tukhoa;
            return View(listTimKiem.OrderBy(n => n.Ten_SP));
        }
    }
}