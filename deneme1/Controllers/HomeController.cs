using Newtonsoft.Json;
using deneme1.Models;
using eticaret.Modeller;
using eticaret.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace deneme1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EticaretContext _db;

        public HomeController(ILogger<HomeController> logger, EticaretContext db)
        {
            _db = db;
            _logger = logger;
        }

       

        public IActionResult Index()
        {
            var urunler = new UrunListesi
            {
                Kategorilerim = _db.AnaKategoris.ToList(),
                Urunlerim=_db.Urunlers.ToList(),
                UrunGorsellerim = _db.UrunGorsels.ToList(),
                Altkategorilerim=_db.AltKategoris.ToList()
            };
            return View(urunler);
        }

        public IActionResult Iletisim()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Urundetay(int? urunid)
        {
            if (urunid == null)
            {
                return RedirectToAction("Magaza","Home");
            }
            var urun= _db.Urunlers.FirstOrDefault(x => x.Id == urunid);
            var detaylar = _db.UrunDetays.Where(x => x.Urunid == urunid).ToList();
            var gorseller = _db.UrunGorsels.Where(x => x.Urunid == urunid).ToList();
            var model= new malzeme
            {
                Urunlerim = new List<Urunler> { urun },
                Urundetaylar�m = detaylar,
                Urungorsellerim = gorseller
            };
            return View(model);
        }
        [HttpGet]
        public IActionResult magaza(int? id, string? min, string? max)
        {
            var urunler = new UrunListesi
            {
                Kategorilerim = _db.AnaKategoris.ToList(),
                UrunGorsellerim = _db.UrunGorsels.ToList(),
                Altkategorilerim = _db.AltKategoris.ToList(),
                UrunDetaylar�m = _db.UrunDetays.ToList()
            };

            // �nce t�m �r�nleri getir
            IQueryable<Urunler> urunQuery = _db.Urunlers;

            // Kategori filtresi
            if (id != null)
            {
                var secilenKategori = _db.AltKategoris.FirstOrDefault(x => x.Id == id);
                if (secilenKategori != null)
                {
                    ViewBag.SecilenKategoriId = id;

                    // Breadcrumb olu�tur
                    var breadcrumbKategoriler = new List<string>();
                    var mevcutKategori = secilenKategori;

                    while (mevcutKategori != null)
                    {
                        breadcrumbKategoriler.Insert(0, mevcutKategori.KategoriAdi);
                        if (mevcutKategori.UstKategoriId != null)
                        {
                            mevcutKategori = _db.AltKategoris
                                .FirstOrDefault(x => x.Id == mevcutKategori.UstKategoriId);
                        }
                        else
                        {
                            var anaKategori = _db.AnaKategoris
                                .FirstOrDefault(x => x.Id == mevcutKategori.AnaKategoriId);
                            if (anaKategori != null)
                                breadcrumbKategoriler.Insert(0, anaKategori.KategoriAdi);
                            break;
                        }
                    }
                    ViewBag.BreadcrumbKategoriler = breadcrumbKategoriler;

                    var kategoriIds = GetTumAltKategoriler(id.Value);
                    urunQuery = urunQuery.Where(x => kategoriIds.Contains(x.KategoriId));
                }
                else
                {
                    var secilenAnaKategori = _db.AnaKategoris.FirstOrDefault(x => x.Id == id);
                    if (secilenAnaKategori != null)
                    {
                        ViewBag.SecilenKategoriId = id;
                        ViewBag.BreadcrumbKategoriler = new List<string> { secilenAnaKategori.KategoriAdi };

                        var altKategoriler = _db.AltKategoris
                            .Where(x => x.AnaKategoriId == id)
                            .ToList();

                        var kategoriIds = new List<int>();
                        foreach (var altKat in altKategoriler)
                        {
                            kategoriIds.AddRange(GetTumAltKategoriler(altKat.Id));
                        }

                        urunQuery = urunQuery.Where(x => kategoriIds.Contains(x.KategoriId));
                    }
                }
            }
            else
            {
                ViewBag.BreadcrumbKategoriler = new List<string> { "T�m �r�nler" };
            }

            if (!string.IsNullOrWhiteSpace(min) && !string.IsNullOrWhiteSpace(max))
            {
                try
                {
                    // $ i�aretini ve bo�luklar� temizle
                    var temizMin = min.Replace("$", "").Replace(" ", "").Replace(",", "").Trim();
                    var temizMax = max.Replace("$", "").Replace(" ", "").Replace(",", "").Trim();
                    if (decimal.TryParse(temizMin, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal decimalMin) &&
                        decimal.TryParse(temizMax, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal decimalMax))
                    {
                       

                        // �ndirimli fiyat varsa onu kullan, yoksa normal sat�� fiyat�n� kullan
                        urunQuery = urunQuery.Where(x =>
                            ((x.IndirimliFiyat.HasValue && x.IndirimliFiyat.Value > 0)
                                ? x.IndirimliFiyat.Value
                                : x.Satis) >= decimalMin &&
                            ((x.IndirimliFiyat.HasValue && x.IndirimliFiyat.Value > 0)
                                ? x.IndirimliFiyat.Value
                                : x.Satis) <= decimalMax
                        );

                        // Breadcrumb'a fiyat bilgisini ekle
                        if (ViewBag.BreadcrumbKategoriler == null)
                        {
                            ViewBag.BreadcrumbKategoriler = new List<string>();
                        }
                        ((List<string>)ViewBag.BreadcrumbKategoriler).Add($"${decimalMin:N2} - ${decimalMax:N2}");
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fiyat filtresi hatas�: {ex.Message}");
                }
            }

            urunler.Urunlerim = urunQuery.ToList();
            return View(urunler);
        }

        // Se�ilen kategori + t�m alt kategorilerini (recursive) d�nd�r�r
        private List<int> GetTumAltKategoriler(int kategoriId)
        {
            var ids = new List<int> { kategoriId }; // �nce kendi ID'sini ekle

            var altKategoriler = _db.AltKategoris
                .Where(x => x.UstKategoriId == kategoriId)
                .ToList();

            foreach (var altKategori in altKategoriler)
            {
                ids.AddRange(GetTumAltKategoriler(altKategori.Id)); // Recursive �a�r�
            }

            return ids;
        }

        [HttpPost]
        public IActionResult Giriss(string mail, string sifre)
        {
            var kullanici = _db.Misafirs.FirstOrDefault(x => x.Mail == mail && x.Sifre == sifre &&x.Durum==true);
            if (kullanici != null)
            {
               
                    HttpContext.Session.SetInt32("UserId", kullanici.Id);
                    HttpContext.Session.SetString("username", kullanici.Isim);
                    return RedirectToAction("Index", "Home");
               
            }
            else
            {
                TempData["Hata"] = "Ge�ersiz mail veya �ifre.";
                return RedirectToAction("Uye", "Home");
            }
        }
        public IActionResult Cikis()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Uye()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Uye(string isim,string mail,string sifre)
        {
            var dogrulamakodu=new Random().Next(1000,9999).ToString();
            var misafir = new Misafir
            {
                Isim = isim,
                Mail = mail,
                Sifre = sifre,
                Kod = dogrulamakodu,
                Durum= false
            };
            _db.Misafirs.Add(misafir);
            _db.SaveChanges();
            // Mail g�nderme
            try
            {
                // Mail ayarlar�
                string gondericiMail = "emreyasiru@gmail.com";
                string gondericiSifre = "rcwv waeh xnum hpjj"; // Gmail uygulama �ifresi

                // SMTP client olu�tur
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(gondericiMail, gondericiSifre);
                smtp.EnableSsl = true;

                // Mail mesaj� olu�tur
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(gondericiMail, "E Ticaret");
                mailMessage.To.Add(mail);
                mailMessage.Subject = "Hesap Do�rulama Kodu";
                mailMessage.Body = $"Merhaba {isim},\n\nHesab�n�z� do�rulamak i�in kod: {dogrulamakodu}\n\n�yi g�nler!";
                mailMessage.IsBodyHtml = false;

                // Mail g�nder
                smtp.Send(mailMessage);

                // Ba�ar�l� mesaj
                TempData["Mesaj"] = "Do�rulama kodu mail adresinize g�nderildi.";
            }
            catch (Exception ex)
            {
                // Hata durumunda
                TempData["Hata"] = "Mail g�nderilirken hata olu�tu: " + ex.Message;
            }

            return RedirectToAction("Dogrula", "Home");
        }
        [HttpGet]
        public IActionResult Dogrula()
        {
          
            return View();
        }
        [HttpPost]
        public IActionResult Dogrula(string kod)
        {
            var onay = _db.Misafirs.FirstOrDefault(x => x.Kod == kod);
            if (onay != null)
            {
                onay.Durum = true;
                _db.SaveChanges();
                HttpContext.Session.SetInt32("UserId", onay.Id);
                HttpContext.Session.SetString("username", onay.Isim);
               
                TempData["Mesaj"] = "Hesab�n�z ba�ar�yla do�ruland�. Giri� yapabilirsiniz.";
                TempData["Basarili"] = "true"; // Ba�ar�l� do�rulama i�in flag
                return View(); // �nce view'� d�nd�r, JavaScript ile y�nlendirme yapaca��z
            }
            else
            {
                TempData["Hata"] = "Hatal� kod girdiniz. L�tfen tekrar deneyin.";
                return View();
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SepeteEkle(int productId, string productName, string brand,
                                decimal price, string imageUrl, int quantity,
                                string color, string size)
        {
            var cart = GetCart();
            cart.AddItem(productId, productName, brand, price, imageUrl, quantity, color, size);
            SaveCart(cart);

            return Json(new { success = true, totalItems = cart.GetTotalItems() });
        }

        [HttpGet]
        public IActionResult SepetAdetAl()
        {
            var cart = GetCart();
            return Json(new { totalItems = cart.GetTotalItems() });
        }

        [HttpPost]
        public IActionResult SepetGuncelle(int productId, string color, string size, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, color, size, quantity);
            SaveCart(cart);

            return Json(new { success = true, totalItems = cart.GetTotalItems(), totalPrice = cart.GetTotalPrice() });
        }

        [HttpPost]
        public IActionResult SepettenSil(int productId, string color, string size)
        {
            var cart = GetCart();
            cart.RemoveItem(productId, color, size);
            SaveCart(cart);

            return Json(new { success = true, totalItems = cart.GetTotalItems(), totalPrice = cart.GetTotalPrice() });
        }

        // Private helper metodlar
        private Cart GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }
            return JsonConvert.DeserializeObject<Cart>(cartJson);
        }

        private void SaveCart(Cart cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        // MEVCUT Sepet metodunu �u �ekilde g�ncelleyin:
        public IActionResult Sepet()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult SepetTemizle()
        {
            var cart = GetCart();
            cart.Clear();
            SaveCart(cart);

            return Json(new { success = true, totalItems = 0, totalPrice = 0 });
        }
    }
}
