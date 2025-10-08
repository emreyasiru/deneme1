using eticaret.Modeller;
using eticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eticaret.Controllers
{
    public class Admin : Controller
    {
        private readonly EticaretContext _db;
        private readonly IWebHostEnvironment _env;

        public Admin(EticaretContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }
            return View();
        }

        public IActionResult Giris()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Kategoriler()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }
            var anaktg = _db.AnaKategoris.ToList();
            var altktg = _db.AltKategoris.ToList();
            var ktg = new Kategori
            {
                AnaKategoriList = anaktg,
                AltKategoriList = altktg
            };
            return View(ktg);
        }

        [HttpGet]
        public IActionResult Urunler()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }

            try
            {
                // Tüm ürünleri tek seferde çek ve kategori bilgilerini left join ile al
                var urunler = (from u in _db.Urunlers
                               select new UrunListeView
                               {
                                   Id = u.Id,
                                   UrunAdi = u.UrunAdi,
                                   Stok = u.Stok,
                                   KategoriId = u.KategoriId,
                                   // Önce AltKategori'den bak, yoksa AnaKategori'den al
                                   KategoriAdi = _db.AltKategoris
                                                   .Where(alt => alt.Id == u.KategoriId)
                                                   .Select(alt => alt.KategoriAdi)
                                                   .FirstOrDefault()
                                               ?? _db.AnaKategoris
                                                   .Where(ana => ana.Id == u.KategoriId)
                                                   .Select(ana => ana.KategoriAdi)
                                                   .FirstOrDefault()
                                               ?? "Kategori Bulunamadı",
                                   Alis = u.Alis,
                                   Satis = u.Satis,
                                   IndirimliFiyat = u.IndirimliFiyat,
                                   Aciklama = u.Aciklama
                               }).ToList();

                return View(urunler);
            }
            catch (Exception ex)
            {
                ViewBag.Hata = $"Ürünler yüklenirken hata oluştu: {ex.Message}";
                return View(new List<UrunListeView>());
            }
        }

        [HttpGet]
        public IActionResult UrunEkle()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }
            var veriler = new UrunKayit
            {
                Vergilerim = _db.Vergis.ToList(),
                Kategorilerim = _db.AnaKategoris.ToList()
            };
            return View(veriler);
        }
        // Detay sayfasını göster
        [HttpGet]
        public IActionResult UrunDetay(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }

            var urun = _db.Urunlers.Find(id);
            if (urun == null)
            {
                TempData["Hata"] = "Ürün bulunamadı!";
                return RedirectToAction("Urunler");
            }

            var model = new UrunDetayViewModel
            {
                UrunId = urun.Id,
                UrunAdi = urun.UrunAdi,
                Detaylar = _db.UrunDetays
                    .Where(d => d.Urunid == id)
                    .ToList()
            };

            return View(model);
        }

        // Yeni detay ekleme
        [HttpPost]
        public IActionResult DetayEkle(int urunId, int? numara, string? beden, string? renk, string? boy, string? marka)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            try
            {
                var detay = new UrunDetay
                {
                    Urunid = urunId,
                    Numara = numara,
                    Beden = beden,      // Virgülle ayrılmış: "XL,S,XXL"
                    Renk = renk,        // Virgülle ayrılmış: "#7b3737,#f52424"
                    Boy = boy,
                    Marka = marka
                };

                _db.UrunDetays.Add(detay);
                _db.SaveChanges();

                return Json(new { success = true, message = "Varyant başarıyla eklendi!", detayId = detay.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Detay silme
        [HttpPost]
        public IActionResult DetaySil(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş." });
            }

            try
            {
                var detay = _db.UrunDetays.Find(id);
                if (detay == null)
                {
                    return Json(new { success = false, message = "Detay bulunamadı!" });
                }

                _db.UrunDetays.Remove(detay);
                _db.SaveChanges();

                return Json(new { success = true, message = "Detay silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UrunEkle(string UrunAdi, string StokAdeti, string Aciklama,
    string AlisFiyati, string SatisFiyati, string IndirimliFiyat, int GercekKategori,
    int Vergi, List<IFormFile> Gorseller, int AnaGorselIndex = 0)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Giris", "Admin");
            }

            try
            {
                // DEBUG
                Console.WriteLine("=== ÜRÜN EKLEME BAŞLADI ===");
                Console.WriteLine($"Ürün Adı: {UrunAdi}");
                Console.WriteLine($"Görsel Sayısı: {Gorseller?.Count ?? 0}");
                Console.WriteLine($"Ana Görsel Index: {AnaGorselIndex}");

                if (Gorseller != null && Gorseller.Count > 0)
                {
                    for (int i = 0; i < Gorseller.Count; i++)
                    {
                        Console.WriteLine($"  Görsel {i}: {Gorseller[i]?.FileName} - {Gorseller[i]?.Length ?? 0} bytes");
                    }
                }
                else
                {
                    Console.WriteLine("  !!! GÖRSEL GELMEDİ !!!");
                }

                // Validasyonlar
                if (string.IsNullOrEmpty(UrunAdi))
                {
                    ViewBag.ErrorMessage = "Ürün adı zorunludur!";
                    return await UrunEklePageReturn();
                }

                if (GercekKategori == 0)
                {
                    ViewBag.ErrorMessage = "Kategori seçimi zorunludur!";
                    return await UrunEklePageReturn();
                }

                bool kategoriMevcut = await _db.AnaKategoris.AnyAsync(x => x.Id == GercekKategori) ||
                                      await _db.AltKategoris.AnyAsync(x => x.Id == GercekKategori);

                if (!kategoriMevcut)
                {
                    ViewBag.ErrorMessage = "Geçersiz kategori seçimi!";
                    return await UrunEklePageReturn();
                }

                // Fiyat ve stok parse işlemleri
                decimal alis = 0, satis = 0, indirimli = 0;
                int stok = 0;

                if (!string.IsNullOrWhiteSpace(AlisFiyati))
                {
                    var temizAlis = AlisFiyati.Replace(",", ".").Trim();
                    decimal.TryParse(temizAlis, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out alis);
                }

                if (!string.IsNullOrWhiteSpace(SatisFiyati))
                {
                    var temizSatis = SatisFiyati.Replace(",", ".").Trim();
                    decimal.TryParse(temizSatis, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out satis);
                }

                if (!string.IsNullOrWhiteSpace(IndirimliFiyat))
                {
                    var temizIndirimli = IndirimliFiyat.Replace(",", ".").Trim();
                    decimal.TryParse(temizIndirimli, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out indirimli);
                }

                if (!string.IsNullOrWhiteSpace(StokAdeti))
                    int.TryParse(StokAdeti, out stok);

                // Yeni ürün oluştur
                var yeniUrun = new Urunler
                {
                    UrunAdi = UrunAdi.Trim(),
                    Stok = stok,
                    KategoriId = GercekKategori,
                    Alis = alis,
                    Satis = satis,
                    IndirimliFiyat = indirimli,
                    Aciklama = string.IsNullOrEmpty(Aciklama) ? "" : Aciklama.Trim(),
                    VergiId = Vergi,
                };

                _db.Urunlers.Add(yeniUrun);
                await _db.SaveChangesAsync();

                Console.WriteLine($"Ürün kaydedildi - ID: {yeniUrun.Id}");

                // Görselleri kaydet
                if (Gorseller != null && Gorseller.Count > 0)
                {
                    Console.WriteLine("Görseller kaydediliyor...");

                    if (AnaGorselIndex < 0 || AnaGorselIndex >= Gorseller.Count)
                    {
                        AnaGorselIndex = 0;
                    }

                    await GorselleriKaydet(yeniUrun.Id, Gorseller, AnaGorselIndex);
                    Console.WriteLine("Görseller başarıyla kaydedildi!");
                }
                else
                {
                    Console.WriteLine("!!! GÖRSEL YOK, ATLANIYOR !!!");
                }

                TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
                return RedirectToAction("Urunler", "Admin");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! HATA: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                ViewBag.ErrorMessage = $"Ürün eklenirken hata oluştu: {ex.Message}";
                return await UrunEklePageReturn();
            }
        }

        private async Task GorselleriKaydet(int urunId, List<IFormFile> gorseller, int anaGorselIndex)
        {
            try
            {
                Console.WriteLine($"\n=== GorselleriKaydet Başladı ===");
                Console.WriteLine($"Ürün ID: {urunId}");
                Console.WriteLine($"Toplam Görsel: {gorseller.Count}");
                Console.WriteLine($"Ana Görsel Index: {anaGorselIndex}");

                // Ana upload klasörünü oluştur
                string uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "urunler", urunId.ToString());
                Console.WriteLine($"Upload Path: {uploadsPath}");

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    Console.WriteLine("Klasör oluşturuldu");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                int gecerliDosyaIndex = 0;

                for (int i = 0; i < gorseller.Count; i++)
                {
                    var gorsel = gorseller[i];

                    Console.WriteLine($"\n--- Görsel {i + 1} işleniyor ---");

                    if (gorsel == null || gorsel.Length == 0)
                    {
                        Console.WriteLine("Görsel boş, atlanıyor");
                        continue;
                    }

                    Console.WriteLine($"Dosya: {gorsel.FileName}");
                    Console.WriteLine($"Boyut: {gorsel.Length} bytes");

                    var fileExtension = Path.GetExtension(gorsel.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        Console.WriteLine($"Geçersiz uzantı: {fileExtension}");
                        continue;
                    }

                    if (gorsel.Length > 5 * 1024 * 1024)
                    {
                        Console.WriteLine("Dosya çok büyük (>5MB)");
                        continue;
                    }

                    string guvenliDosyaAdi = $"urun_{urunId}_{DateTime.Now.Ticks}_{i}{fileExtension}";
                    string dosyaYolu = Path.Combine(uploadsPath, guvenliDosyaAdi);

                    Console.WriteLine($"Kaydedilecek: {guvenliDosyaAdi}");

                    // Dosyayı kaydet
                    using (var stream = new FileStream(dosyaYolu, FileMode.Create))
                    {
                        await gorsel.CopyToAsync(stream);
                    }

                    Console.WriteLine("Dosya fiziksel olarak kaydedildi");

                    bool anaGorselMi = (gecerliDosyaIndex == anaGorselIndex);
                    Console.WriteLine($"Ana görsel mi?: {anaGorselMi} (Index: {gecerliDosyaIndex} == {anaGorselIndex})");

                    // Veritabanına kaydet
                    var urunGorsel = new UrunGorsel
                    {
                        Urunid = urunId,
                        Ad = guvenliDosyaAdi,
                        Baslangic = anaGorselMi
                    };

                    _db.UrunGorsels.Add(urunGorsel);
                    Console.WriteLine($"DB'ye eklendi - Baslangic: {anaGorselMi}");

                    gecerliDosyaIndex++;
                }

                Console.WriteLine($"\nToplam {gecerliDosyaIndex} görsel işlendi");
                Console.WriteLine("SaveChangesAsync çağrılıyor...");

                await _db.SaveChangesAsync();

                Console.WriteLine("SaveChanges BAŞARILI!");

                // Kontrol için kayıtları listele
                var kaydedilenler = await _db.UrunGorsels.Where(x => x.Urunid == urunId).ToListAsync();
                Console.WriteLine($"\nKaydedilen görsel sayısı: {kaydedilenler.Count}");
                foreach (var g in kaydedilenler)
                {
                    Console.WriteLine($"  ID: {g.Id}, Ad: {g.Ad}, Baslangic: {g.Baslangic}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n!!! GÖRSELLERİ KAYDETME HATASI !!!");
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");

                // Hata durumunda dosyaları temizle
                string uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "urunler", urunId.ToString());
                if (Directory.Exists(uploadsPath))
                {
                    try
                    {
                        Directory.Delete(uploadsPath, true);
                        Console.WriteLine("Hatalı dosyalar temizlendi");
                    }
                    catch (Exception deleteEx)
                    {
                        Console.WriteLine($"Temizleme hatası: {deleteEx.Message}");
                    }
                }

                throw new Exception($"Görseller kaydedilirken hata: {ex.Message}");
            }
        }

        private async Task<IActionResult> UrunEklePageReturn()
        {
            var veriler = new UrunKayit
            {
                Vergilerim = await _db.Vergis.ToListAsync(),
                Kategorilerim = await _db.AnaKategoris.ToListAsync()
            };
            return View("UrunEkle", veriler);
        }

        [HttpGet]
        public IActionResult KategoriGetir(int id)
        {
            var gelen = _db.AltKategoris
                .Where(x => x.AnaKategoriId == id && x.UstKategoriId == null)
                .ToList();

            var secilenKategori = _db.AnaKategoris.FirstOrDefault(x => x.Id == id);

            var html = "";

            if (secilenKategori != null)
            {
                html += "<div class='alert alert-info mt-2 secilen-kategori'>";
                html += "<strong>Seçilen:</strong> " + secilenKategori.KategoriAdi;
                html += "</div>";
            }

            if (gelen.Count > 0)
            {
                html += "<select class='form-select mt-2' onchange='secimim(this)' data-seviye='2'>";
                html += "<option value=''>Alt kategori seçiniz</option>";
                foreach (var item in gelen)
                {
                    html += $"<option value='{item.Id}'>{item.KategoriAdi}</option>";
                }
                html += "</select>";
            }

            return Content(html, "text/html");
        }

        [HttpGet]
        public IActionResult AltKategoriGetir(int id)
        {
            var gelen = _db.AltKategoris.Where(x => x.UstKategoriId == id).ToList();
            var secilenAltKategori = _db.AltKategoris.FirstOrDefault(x => x.Id == id);

            var html = "";

            if (secilenAltKategori != null)
            {
                html += "<div class='alert alert-success mt-2 secilen-kategori'>";
                html += "<strong>Seçilen:</strong> " + secilenAltKategori.KategoriAdi;
                html += "</div>";
            }

            if (gelen.Count > 0)
            {
                html += "<select class='form-select mt-2' onchange='secimim(this)' data-seviye='3'>";
                html += "<option value=''>Alt kategori seçiniz</option>";
                foreach (var item in gelen)
                {
                    html += $"<option value='{item.Id}'>{item.KategoriAdi}</option>";
                }
                html += "</select>";
            }

            return Content(html, "text/html");
        }
        [HttpPost]
        public IActionResult KategoriEkle(string Kategori_Adi, string? altkategori, string anakategori)
        {
            if (anakategori == "0")
            {
                var ktgekle = new AnaKategori
                {
                    KategoriAdi = Kategori_Adi
                };
                _db.AnaKategoris.Add(ktgekle);
                _db.SaveChanges();
                ViewBag.hata = "kategori ekleme işlemi tamamlanmıştır";
            }
            else
            {
                int anaktgid = Convert.ToInt32(anakategori);
                if (altkategori == "0")
                {
                    var altktg = new AltKategori
                    {
                        AnaKategoriId = anaktgid,
                        KategoriAdi = Kategori_Adi,
                        Durum = true,
                        UstKategoriId = null
                    };
                    _db.AltKategoris.Add(altktg);
                }
                else
                {
                    int ustktgid = Convert.ToInt32(altkategori);
                    var altktg = new AltKategori
                    {
                        AnaKategoriId = anaktgid,
                        KategoriAdi = Kategori_Adi,
                        Durum = true,
                        UstKategoriId = ustktgid,
                    };
                    _db.AltKategoris.Add(altktg);
                }


                _db.SaveChanges();

            }
            return RedirectToAction("Kategoriler", "Admin");
        }

        [HttpPost]
        public IActionResult Giris(string username, string password)
        {
            var dogrula = _db.Kullanicis.FirstOrDefault(x => x.Username == username && x.Password == password);
            if (dogrula != null)
            {
                if (dogrula.Durum == false)
                {
                    ViewBag.ErrorMessage = "kullanıcı hesabınız devre dışı bırakılmış";
                    return View();
                }

                HttpContext.Session.SetInt32("UserId", dogrula.Id);
                HttpContext.Session.SetString("username", username);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.ErrorMessage = "kullanıcı adı veya şifre yanlış";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> UrunSil(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Oturum süresi dolmuş. Lütfen tekrar giriş yapın." });
            }

            try
            {
                // Ürünü bul
                var urun = await _db.Urunlers.FirstOrDefaultAsync(x => x.Id == id);
                if (urun == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                // Ürüne ait görselleri bul
                var urunGorselleri = _db.UrunGorsels.Where(x => x.Urunid == id).ToList();

                // Görselleri dosya sisteminden sil
                if (urunGorselleri.Any())
                {
                    string uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "urunler", id.ToString());

                    foreach (var gorsel in urunGorselleri)
                    {
                        string dosyaYolu = Path.Combine(uploadsPath, gorsel.Ad);
                        if (System.IO.File.Exists(dosyaYolu))
                        {
                            System.IO.File.Delete(dosyaYolu);
                        }
                    }

                    // Klasörü sil (boşsa)
                    if (Directory.Exists(uploadsPath) && !Directory.EnumerateFileSystemEntries(uploadsPath).Any())
                    {
                        Directory.Delete(uploadsPath);
                    }

                    // Veritabanından görselleri sil
                    _db.UrunGorsels.RemoveRange(urunGorselleri);
                }

                // Ürünü veritabanından sil
                _db.Urunlers.Remove(urun);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Ürün başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ürün silinirken hata oluştu: {ex.Message}" });
            }
        }

        public IActionResult deneme()
        {
            return View();
        }
    }
}