using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Lab03_BT
{
    [DataContract]
    public class SinhVien
    {
        [DataMember] public string MSSV { get; set; }
        [DataMember] public string HoTenLot { get; set; }
        [DataMember] public string Ten { get; set; }
        [DataMember] public DateTime NgaySinh { get; set; }
        [DataMember] public string GioiTinh { get; set; }
        [DataMember] public string Lop { get; set; }
        [DataMember] public string CMND { get; set; }
        [DataMember] public string SDT { get; set; }
        [DataMember] public string DiaChi { get; set; }
        // Danh sách môn học sinh viên đăng ký
        [DataMember] public List<string> MonHocDangKy { get; set; }
        public SinhVien()
        {
            MonHocDangKy = new List<string>();
        }
        public string HoTenDayDu
        {
            get { return (HoTenLot + " " + Ten); }
        }
    }
}