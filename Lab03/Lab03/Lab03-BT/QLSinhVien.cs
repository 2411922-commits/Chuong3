using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Text;
namespace Lab03_BT
{
    public class QLSinhVien
    {
        private readonly string thuMuc;
        public List<SinhVien> DanhSach { get; private set; }
        public QLSinhVien()
        {
            thuMuc = AppDomain.CurrentDomain.BaseDirectory;
            DanhSach = new List<SinhVien>();
            string fileTxt = Path.Combine(thuMuc, "students.txt");
            string fileCu = Path.Combine(thuMuc, "sinhvien.txt");
            // Ưu tiên file mới, nếu chưa có thì đọc file cũ.
            if (File.Exists(fileTxt))
                DocFile("txt");
            else if (File.Exists(fileCu))
                DocFileCu(fileCu);
            else
            {
                LuuFile("txt");
            }
        }
        // Đọc dữ liệu từ định dạng được chọn.
        public void DocFile(string dinhDang)
        {
            string duoiFile = dinhDang.Trim().ToLowerInvariant();
            if (duoiFile.StartsWith("."))
                duoiFile = duoiFile.Substring(1);
            string tenFile;
            switch (duoiFile)
            {
                case "txt":
                    tenFile = "students.txt";
                    break;
                case "xml":
                    tenFile = "students.xml";
                    break;
                case "json":
                    tenFile = "students.json";
                    break;
                default:
                    throw new ArgumentException("Chỉ hỗ trợ TXT, XML và JSON.");
            }
            string duongDan = Path.Combine(thuMuc, tenFile);
            if (!File.Exists(duongDan))
                throw new FileNotFoundException(
                    "Không tìm thấy tập tin: " + tenFile);
            List<SinhVien> danhSachDoc;
            if (duoiFile == "txt")
            {
                danhSachDoc = DocTXT(duongDan);
            }
            else if (duoiFile == "xml")
            {
                XmlSerializer serializer =new XmlSerializer(typeof(List<SinhVien>));
                using (FileStream fs = File.OpenRead(duongDan))
                {
                    danhSachDoc =(List<SinhVien>)serializer.Deserialize(fs);
                }
            }
            else
            {
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(
                        typeof(List<SinhVien>));
                using (FileStream fs = File.OpenRead(duongDan))
                {
                    danhSachDoc =(List<SinhVien>)serializer.ReadObject(fs);
                }
            }
            DanhSach = danhSachDoc ?? new List<SinhVien>();
            foreach (SinhVien sv in DanhSach)
            {
                if (sv.MonHocDangKy == null)
                    sv.MonHocDangKy = new List<string>();
            }
        }
        // Ghi dữ liệu vào định dạng được chọn.
        public void LuuFile(string dinhDang)
        {
            string duoiFile = dinhDang.Trim().ToLowerInvariant();
            if (duoiFile.StartsWith("."))
                duoiFile = duoiFile.Substring(1);
            string tenFile;
            switch (duoiFile)
            {
                case "txt":
                    tenFile = "students.txt";
                    break;
                case "xml":
                    tenFile = "students.xml";
                    break;
                case "json":
                    tenFile = "students.json";
                    break;
                default:
                    throw new ArgumentException(
                        "Chỉ hỗ trợ TXT, XML và JSON.");
            }
            string duongDan = Path.Combine(thuMuc, tenFile);
            if (duoiFile == "txt")
            {
                LuuTXT(duongDan);
            }
            else if (duoiFile == "xml")
            {
                XmlSerializer serializer =new XmlSerializer(typeof(List<SinhVien>));
                using (FileStream fs = File.Create(duongDan))
                {
                    serializer.Serialize(fs, DanhSach);
                }
            }
            else
            {
                DataContractJsonSerializer serializer =new DataContractJsonSerializer(typeof(List<SinhVien>));
                using (FileStream fs = File.Create(duongDan))
                {
                    serializer.WriteObject(fs, DanhSach);
                }
            }
        }
        // Giữ nguyên LuuFile() để các nút hiện tại không cần sửa.
        // Mỗi lần lưu sẽ cập nhật cả ba tập tin.
        public void LuuFile()
        {
            LuuFile("txt");
            LuuFile("xml");
            LuuFile("json");
        }
        // Đọc TXT theo định dạng cũ.
        private List<SinhVien> DocTXT(string duongDan)
        {
            List<SinhVien> ketQua = new List<SinhVien>();
            foreach (string line in File.ReadAllLines(duongDan))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string[] p = line.Split('|');
                if (p.Length != 10)
                    continue;
                DateTime ngaySinh;
                if (!DateTime.TryParseExact(p[3],"dd/MM/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out ngaySinh))
                {
                    continue;
                }
                SinhVien sv = new SinhVien
                {
                    MSSV = p[0],
                    HoTenLot = p[1],
                    Ten = p[2],
                    NgaySinh = ngaySinh,
                    GioiTinh = p[4],
                    Lop = p[5],
                    CMND = p[6],
                    SDT = p[7],
                    DiaChi = p[8],
                    MonHocDangKy = p[9]
                        .Split(new char[] { ';' },
                            StringSplitOptions.RemoveEmptyEntries)
                        .ToList()
                };
                ketQua.Add(sv);
            }
            return ketQua;
        }
        // Ghi TXT theo định dạng cũ.
        private void LuuTXT(string duongDan)
        {
            var lines = DanhSach.Select(sv =>
                string.Join("|", new string[]
                {
                    sv.MSSV ?? "",
                    sv.HoTenLot ?? "",
                    sv.Ten ?? "",
                    sv.NgaySinh.ToString("dd/MM/yyyy"),
                    sv.GioiTinh ?? "",
                    sv.Lop ?? "",
                    sv.CMND ?? "",
                    sv.SDT ?? "",
                    sv.DiaChi ?? "",
                    string.Join(";", sv.MonHocDangKy
                        ?? new List<string>())
                }));
            File.WriteAllLines(duongDan, lines);
        }
        // Đọc file sinhvien.txt cũ để chuyển sang cấu trúc mới.
        private void DocFileCu(string duongDan)
        {
            DanhSach = DocTXT(duongDan);
            LuuFile();
        }

        public void DocFileTuDuongDan(string duongDan, string dinhDang)
        {
            string duoiFile = dinhDang.Trim().ToLowerInvariant();
            if (duoiFile.StartsWith("."))
                duoiFile = duoiFile.Substring(1);
            List<SinhVien> danhSachDoc;
            if (duoiFile == "txt")
            {
                danhSachDoc = DocTXT(duongDan);
            }
            else if (duoiFile == "xml")
            {
                XmlSerializer serializer =new XmlSerializer(typeof(List<SinhVien>));
                using (FileStream fs = File.OpenRead(duongDan))
                {
                    danhSachDoc =(List<SinhVien>)serializer.Deserialize(fs);
                }
            }
            else if (duoiFile == "json")
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<SinhVien>));
                using (FileStream fs = File.OpenRead(duongDan))
                {
                    danhSachDoc =(List<SinhVien>)serializer.ReadObject(fs);
                }
            }
            else
            {
                throw new ArgumentException("Chỉ hỗ trợ tệp TXT, XML và JSON.");
            }
            DanhSach = danhSachDoc ?? new List<SinhVien>();
            foreach (SinhVien sv in DanhSach)
            {
                if (sv.MonHocDangKy == null)
                    sv.MonHocDangKy = new List<string>();
            }
        }
    }
}
