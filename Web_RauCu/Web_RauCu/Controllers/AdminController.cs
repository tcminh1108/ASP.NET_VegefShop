using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_RauCu.Models;
using PagedList;
using PagedList.Mvc;
using System.IO;
using System.Data.OleDb;
using System.Data;
using LinqToExcel;
using System.Data.Entity.Validation;

namespace Web_RauCu.Controllers
{
    
    public class AdminController : Controller
    {
        dbQLNongsanDataContext data = new dbQLNongsanDataContext();
        // GET: Admin
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
        public ActionResult DangKy(FormCollection collection, QuanTri qt)
        {
            var hoten = collection["Hoten_admin_dk"];
            var taikhoan = collection["Taikhoan_admin_dk"];
            var matkhau = collection["Matkhau_admin_dk"];
            var nhaplaimatkhau = collection["Nhaplaimatkhau_admin_dk"];
            var kttk = data.QuanTris.SingleOrDefault(n => n.Taikhoan_QT == taikhoan);
            if (kttk != null)
            {
                ViewBag.Thongbao = "Tài Khoản đã tồn tại!";
            }
            else
            {
                
                qt.Hoten_QT = hoten;
                qt.Taikhoan_QT = taikhoan;
                qt.Matkhau_QT = matkhau;
                data.QuanTris.InsertOnSubmit(qt);
                data.SubmitChanges();
                return RedirectToAction("DangNhap", "Admin");
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
            var taikhoan = collection["taikhoan_admin_dn"];
            var matkhau = collection["matkhau_admin_dn"];
            var qt = data.QuanTris.SingleOrDefault(n => n.Taikhoan_QT == taikhoan && n.Matkhau_QT == matkhau);
            if (qt != null)
            {
                Session["taikhoanAdmin"] = qt;
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.Thongbao = "Tài khoản hoặc mật khẩu không hợp lệ!";
                return this.DangNhap();
            }

        }
        //----------Quản lý sản phẩm----------
        public ActionResult SanPham(int? page)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                int pagesize = 8;
                int pagenum = (page ?? 1);
                var ListSP = data.SanPhams.ToList().OrderByDescending(n => n.Ma_SP).ToPagedList(pagenum, pagesize);
                return View(ListSP);
            }
        }


        [HttpGet]
        public ActionResult ThemSanPham()
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                ViewBag.Ma_NCC = new SelectList(data.NhaCungCaps.ToList().OrderBy(n => n.Ten_NCC), "Ma_NCC", "Ten_NCC");
                ViewBag.Ma_LSP = new SelectList(data.LoaiSanPhams.ToList().OrderBy(n => n.Ten_LSP), "Ma_LSP", "Ten_LSP");
                return View();
            }
        }

        [HttpPost]
        public ActionResult ThemSanPham(FormCollection collection, HttpPostedFileBase FileUpload)
        {
            string filename = "";
            string targetpath = Server.MapPath("~/Assets/doc/");
            var path = Path.Combine(targetpath, FileUpload.FileName);
            if (System.IO.File.Exists(path))
            {
                string extensionName = Path.GetExtension(FileUpload.FileName);
                filename = FileUpload.FileName + DateTime.Now.ToString("ddMMyyyy") + extensionName;
                path = Path.Combine(targetpath, filename);
                FileUpload.SaveAs(path);
            }
            else
            {
                filename = FileUpload.FileName;
                FileUpload.SaveAs(targetpath + filename);
            }

            string pathToExcelFile = targetpath + filename;
            var connectionString = "";
            if (filename.EndsWith(".xls"))
            {
                connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
            }
            else if (filename.EndsWith(".xlsx"))
            {
                connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
            }

            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "ExcelTable");
            DataTable dtable = ds.Tables["ExcelTable"];
            string sheetName = "Sheet1";
            var excelFile = new ExcelQueryFactory(pathToExcelFile);
            var artistAlbums = from a in excelFile.Worksheet<SanPham>(sheetName) select a;
            foreach (var a in artistAlbums)
            {
                try
                {
                    if (a.Ma_SP != -1 && Check_MaSP(a.Ma_SP) == -1 && a.Ten_SP != "" && a.Hinhanh != "" && a.Giaban != 0 && a.Soluongton != 0)
                    {
                        SanPham sp = new SanPham();
                        var nhacungcap = int.Parse(collection["Ma_NCC"]);
                        var loaisanpham = int.Parse(collection["Ma_LSP"]);
                        sp.Ma_SP = a.Ma_SP;
                        sp.Ma_LSP = loaisanpham;
                        sp.Ma_NCC = nhacungcap;
                        sp.Ten_SP = a.Ten_SP;                       
                        sp.Giaban = a.Giaban;
                        sp.Hinhanh = a.Hinhanh;
                        sp.Ngaydangban = DateTime.Now;
                        sp.Soluongton = a.Soluongton;
                        sp.Tinhtrang = true;
                        data.SanPhams.InsertOnSubmit(sp);
                        data.SubmitChanges();
                    }
                    else
                    {

                        TempData["message"] = "Nhập danh sách sinh viên KHÔNG thành công. Vui lòng kiểm tra lại";
                        TempData["alert"] = "alert-danger";
                        return RedirectToAction("ThemSanPham", "QuanTri");
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        }
                    }
                }
            }
            return RedirectToAction("SanPham", "Admin");

        }
        public int Check_MaSP(int MaSP)
        {
            if (data.SanPhams.Count(x => x.Ma_SP == MaSP) > 0)
            {
                return data.SanPhams.FirstOrDefault(x => x.Ma_SP == MaSP).Ma_SP;
            }
            else
            {
                return -1;
            }
        }
        public ActionResult XoaSanPham(int Ma_SP)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                SanPham sp = data.SanPhams.SingleOrDefault(n => n.Ma_SP == Ma_SP);
                data.SanPhams.DeleteOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("SanPham", "Admin");
            }
        }
        [HttpGet]
        public ActionResult SuaSanPham(int Ma_SP)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                SanPham sp = data.SanPhams.SingleOrDefault(n => n.Ma_SP == Ma_SP);
                ViewBag.Ma_LSP = new SelectList(data.LoaiSanPhams.ToList().OrderBy(n => n.Ten_LSP), "Ma_LSP", "Ten_LSP", sp.Ma_LSP);
                ViewBag.Ma_NCC = new SelectList(data.NhaCungCaps.ToList().OrderBy(n => n.Ten_NCC), "Ma_NCC", "Ten_NCC", sp.Ma_NCC);
                return View(sp);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaSanPham(int Ma_SP, SanPham sp, FormCollection collection)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                if (ModelState.IsValid)
                {
                    sp = data.SanPhams.SingleOrDefault(n => n.Ma_SP == Ma_SP);
                    var tensp = collection["tensp"];
                    var giaban = int.Parse(collection["giaban"]);
                    var soluong = int.Parse(collection["soluong"]);
                    var loaisanpham = int.Parse(collection["Ma_LSP"]);
                    var nhacungcap = int.Parse(collection["Ma_NCC"]);
                    /*var tinhtrang = collection["Tinhtrang"];*/
                    sp.Giaban = giaban;
                    sp.Ngaydangban = DateTime.Now;
                    sp.Soluongton = soluong;
                    sp.Ma_LSP = loaisanpham;
                    sp.Ma_NCC = nhacungcap;
                    sp.Tinhtrang = true;
                    UpdateModel(sp);
                    data.SubmitChanges();
                }
                return RedirectToAction("SanPham", "Admin");
            }
        }
        //----------Quản lý nhà cung cấp----------
        public ActionResult NhaCungCap(int? page)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                int pagesize = 8;
                int pagenum = (page ?? 1);
                var ListNCC = data.NhaCungCaps.ToList().OrderByDescending(n => n.Ma_NCC).ToPagedList(pagenum, pagesize);
                return View(ListNCC);
            }
        }
        public ActionResult XoaNhaCungCap(int Ma_NCC)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                NhaCungCap ncc = data.NhaCungCaps.SingleOrDefault(n => n.Ma_NCC == Ma_NCC);
                data.NhaCungCaps.DeleteOnSubmit(ncc);
                data.SubmitChanges();
                return RedirectToAction("NhaCungCap", "Admin");
            }
        }

        [HttpGet]
        public ActionResult ThemNhaCungCap()
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                return View();
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemNhaCungCap(NhaCungCap ncc, FormCollection collection)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                if (ModelState.IsValid)
                {
                    var tenncc = collection["tenncc"];
                    var gmail = collection["gmail"];
                    var diachi = collection["diachi"];
                    var dienthoai = collection["dienthoai"];
                    ncc.Ten_NCC = tenncc;
                    ncc.Gmail_NCC = gmail;
                    ncc.Diachi_NCC = diachi;
                    ncc.Dienthoai_NCC = dienthoai;
                    data.NhaCungCaps.InsertOnSubmit(ncc);
                    data.SubmitChanges();
                }
                return RedirectToAction("NhaCungCap", "Admin");
            }
        }
        [HttpGet]
        public ActionResult SuaNhaCungCap(int Ma_NCC)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                NhaCungCap ncc = data.NhaCungCaps.SingleOrDefault(n => n.Ma_NCC == Ma_NCC);
                return View(ncc);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaNhaCungCap(int Ma_NCC, NhaCungCap ncc, FormCollection collection)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                if (ModelState.IsValid)
                {
                    ncc = data.NhaCungCaps.SingleOrDefault(n => n.Ma_NCC == Ma_NCC);
                    var tenncc = collection["tenncc"];
                    var gmail = collection["gmail"];
                    var diachi = collection["diachi"];
                    var dienthoai = collection["sdt"];
                    ncc.Ten_NCC = tenncc;
                    ncc.Gmail_NCC = gmail;
                    ncc.Diachi_NCC = diachi;
                    ncc.Dienthoai_NCC = dienthoai;
                    UpdateModel(ncc);
                    data.SubmitChanges();
                }
                return RedirectToAction("NhaCungCap", "Admin");
            }
        }

        //----------Quản lý loại sản phẩm----------
        public ActionResult LoaiSanPham(int? page)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                int pagesize = 8;
                int pagenum = (page ?? 1);
                var ListLSP = data.LoaiSanPhams.ToList().OrderByDescending(n => n.Ma_LSP).ToPagedList(pagenum, pagesize);
                return View(ListLSP);
            }
        }
        public ActionResult XoaLoaiSanPham(int Ma_LSP)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                LoaiSanPham lsp = data.LoaiSanPhams.SingleOrDefault(n => n.Ma_LSP == Ma_LSP);
                data.LoaiSanPhams.DeleteOnSubmit(lsp);
                data.SubmitChanges();
                return RedirectToAction("LoaiSanPham", "Admin");
            }
        }

        [HttpGet]
        public ActionResult ThemLoaiSanPham()
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                return View();
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemLoaiSanPham(LoaiSanPham lsp, FormCollection collection)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                if (ModelState.IsValid)
                {
                    var tenlsp = collection["tenlsp"];
                    lsp.Ten_LSP = tenlsp;
                    data.LoaiSanPhams.InsertOnSubmit(lsp);
                    data.SubmitChanges();
                }
                return RedirectToAction("LoaiSanPham", "Admin");
            }
        }
        [HttpGet]
        public ActionResult SuaLoaiSanPham(int Ma_LSP)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                LoaiSanPham lsp = data.LoaiSanPhams.SingleOrDefault(n => n.Ma_LSP == Ma_LSP);
                return View(lsp);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaLoaiSanPham(int Ma_LSP, LoaiSanPham lsp, FormCollection collection)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                if (ModelState.IsValid)
                {
                    lsp = data.LoaiSanPhams.SingleOrDefault(n => n.Ma_LSP == Ma_LSP);
                    var tenlsp = collection["tenlsp"];
                    lsp.Ten_LSP = tenlsp;
                    UpdateModel(lsp);
                    data.SubmitChanges();
                }
                return RedirectToAction("LoaiSanPham", "Admin");
            }
        }

        //----------Quản lý đơn hàng----------
        public ActionResult DonDatHang(int? page)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                int pagesize = 8;
                int pagenum = (page ?? 1);
                var ListDDH = data.DonDatHangs.ToList().OrderByDescending(n => n.Ma_DDH).ToPagedList(pagenum, pagesize);
                return View(ListDDH);
            }
        }
        public ActionResult CT_DDH(int Ma_DDH)
        {
            if (Session["taikhoanAdmin"] == null)
                return RedirectToAction("DangNhap", "Admin");
            else
            {
                var ListCT_DDH = data.CT_DonDatHangs.Where(n => n.Ma_DDH == Ma_DDH).ToList();
                return View(ListCT_DDH);
            }
        }
    }
}