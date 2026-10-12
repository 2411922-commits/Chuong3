using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Lab03_BT
{
    public partial class Form1 : Form
    {
        private QLSinhVien qly = new QLSinhVien();
        private string mssvBanDau = null;
        public Form1()
        {
            InitializeComponent();
            //nhấn chuột phải lên bảng hiện menu
            dgvSinhVien.ContextMenuStrip = cmsSinhVien;
        }
        private void HienThiDanhSach()
    {
        // Hiển thị dữ liệu sinh viên
        dgvSinhVien.AutoGenerateColumns = true;
        dgvSinhVien.DataSource = null;
        dgvSinhVien.DataSource = qly.DanhSach;
        // Tạo cột checkbox nếu chưa có
        if (!dgvSinhVien.Columns.Contains("Chon"))
        {
            DataGridViewCheckBoxColumn cotChon =new DataGridViewCheckBoxColumn();
            cotChon.Name = "Chon";
            cotChon.HeaderText = "Chọn";
            cotChon.Width = 50;
            cotChon.ReadOnly = false;
            cotChon.TrueValue = true;
            cotChon.FalseValue = false;
            dgvSinhVien.Columns.Insert(0, cotChon);
        }
        // Cho phép tích checkbox
        dgvSinhVien.ReadOnly = false;
        dgvSinhVien.Columns["Chon"].ReadOnly = false;
        dgvSinhVien.Columns["Chon"].Visible = true;
        dgvSinhVien.Columns["Chon"].Width = 50;
        // Thiết lập bảng
        dgvSinhVien.AllowUserToAddRows = false;
        dgvSinhVien.EditMode =DataGridViewEditMode.EditOnEnter;
    }
        private void Form1_Load(object sender, EventArgs e)
        {
            HienThiDanhSach();
        }
        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            DataGridViewRow row = dgvSinhVien.Rows[e.RowIndex];
            if (row.DataBoundItem == null)
                return;
            SinhVien sv = row.DataBoundItem as SinhVien;
            if (sv == null)
                return;
            txtMSSV.Text = sv.MSSV;
            mssvBanDau = sv.MSSV;
            txtHoTenLot.Text = sv.HoTenLot;
            txtTen.Text = sv.Ten;
            dtpNgaySinh.Value = sv.NgaySinh;
            txtCMND.Text = sv.CMND;
            txtDiaChi.Text = sv.DiaChi;
            mtxtSoDT.Text = sv.SDT;
            rdNam.Checked = sv.GioiTinh == "Nam";
            rdNu.Checked = sv.GioiTinh == "Nu"
                        || sv.GioiTinh == "Nữ";
            cboLop.SelectedItem = sv.Lop;
            // Hiển thị các môn học đã đăng ký từ file TXT
            for (int i = 0; i < clbMonHoc.Items.Count; i++)
            {
                string tenMonTrenForm =clbMonHoc.Items[i].ToString().Trim();
                bool daDangKy = sv.MonHocDangKy != null &&
                    sv.MonHocDangKy.Any(mon =>
                        string.Equals(
                            mon.Trim(),
                            tenMonTrenForm,
                            StringComparison.OrdinalIgnoreCase));
                clbMonHoc.SetItemChecked(i, daDangKy);
            }
        }
        private void XoaTrangNhapLieu()
        {
            txtMSSV.Clear();
            txtHoTenLot.Clear();
            txtTen.Clear();
            txtCMND.Clear();
            mtxtSoDT.Clear();
            txtDiaChi.Clear();
            dtpNgaySinh.Value = DateTime.Today;
            rdNam.Checked = false;
            rdNu.Checked = false;
            cboLop.SelectedIndex = -1;
            cboLop.Text = "";
            for (int i = 0; i < clbMonHoc.Items.Count; i++)
            {
                clbMonHoc.SetItemChecked(i, false);
            }
            mssvBanDau = null;
            txtMSSV.Focus();
        }
        private bool KiemTraMSSVTheoLop(string mssv, string lop, out string thongBao)
        {
            thongBao = "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(mssv ?? "", @"^\d{7}$"))
            {
                thongBao = "MSSV phải gồm đúng 7 chữ số!";
                return false;
            }
            if (mssv.Substring(2, 2) != "10")
            {
                thongBao = "Hai chữ số thứ 3 và thứ 4 của MSSV phải là 10!";
                return false;
            }
            var ketQua = System.Text.RegularExpressions.Regex.Match(lop ?? "", @"^CTK(\d{2})");
            if (!ketQua.Success)
            {
                thongBao = "Không xác định được năm nhập học từ lớp!";
                return false;
            }
            int khoa = int.Parse(ketQua.Groups[1].Value);
            int namNhapHoc = khoa - 24;
            string haiSoNam = namNhapHoc.ToString("D2");
            if (mssv.Substring(0, 2) != haiSoNam)
            {
                thongBao = "MSSV không đúng năm nhập học của lớp "
                    + lop + ". Hai số đầu phải là " + haiSoNam + ".";
                return false;
            }
            return true;
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra các thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(txtMSSV.Text) ||
                string.IsNullOrWhiteSpace(txtHoTenLot.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                string.IsNullOrWhiteSpace(cboLop.Text) ||
                string.IsNullOrWhiteSpace(txtCMND.Text) ||
                string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
                clbMonHoc.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ thông tin và chọn ít nhất một môn học!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            // 2. Kiểm tra định dạng MSSV
            string mssv = txtMSSV.Text.Trim();
            string loiMSSV;
            if (!KiemTraMSSVTheoLop(
                mssv, cboLop.Text.Trim(), out loiMSSV))
            {
                MessageBox.Show(loiMSSV, "MSSV không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMSSV.Focus();
                return;
            }
            // 3. Kiểm tra CMND
            string cmnd = txtCMND.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                cmnd, @"^\d{9}$"))
            {
                MessageBox.Show("CMND phải gồm đúng 9 chữ số!");
                txtCMND.Focus();
                return;
            }
            // 4. Kiểm tra số điện thoại
            string sdt = System.Text.RegularExpressions.Regex.Replace(
                mtxtSoDT.Text, @"\D", "");
            if (sdt.Length != 10)
            {
                MessageBox.Show(
                    "Số điện thoại phải gồm đúng 10 chữ số!");
                mtxtSoDT.Focus();
                return;
            }
            // 5. Kiểm tra MSSV đã tồn tại chưa
            if (qly.DanhSach.Any(sv =>
                sv.MSSV.Equals(
                    mssv, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    "MSSV này đã tồn tại! Vui lòng nhập mã khác.");
                txtMSSV.Focus();
                return;
            }
            // 6. Lấy danh sách môn học đã chọn
            List<string> dsMonHoc = clbMonHoc.CheckedItems
                .Cast<object>()
                .Select(mon => mon.ToString())
                .ToList();
            // 7. Tạo đối tượng sinh viên mới
            SinhVien svMoi = new SinhVien
            {
                MSSV = mssv,
                HoTenLot = txtHoTenLot.Text.Trim(),
                Ten = txtTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value.Date,
                GioiTinh = rdNam.Checked ? "Nam" : "Nữ",
                Lop = cboLop.Text,
                CMND = cmnd,
                SDT = sdt,
                DiaChi = txtDiaChi.Text.Trim(),
                MonHocDangKy = dsMonHoc
            };
            // 8. Thêm sinh viên và lưu vào tệp
            qly.DanhSach.Add(svMoi);
            qly.LuuFile();
            // 9. Cập nhật lại bảng
            HienThiDanhSach();
            XoaTrangNhapLieu();
            MessageBox.Show(
                "Thêm sinh viên thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(mssvBanDau))
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần cập nhật trong danh sách!");
                return;
            }
            // Kiểm tra thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(txtMSSV.Text) ||
                string.IsNullOrWhiteSpace(txtHoTenLot.Text) ||
                string.IsNullOrWhiteSpace(txtTen.Text) ||
                string.IsNullOrWhiteSpace(cboLop.Text) ||
                string.IsNullOrWhiteSpace(txtCMND.Text) ||
                string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
                clbMonHoc.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ thông tin và chọn ít nhất một môn học!");
                return;
            }
            string mssvMoi = txtMSSV.Text.Trim();
            string cmnd = txtCMND.Text.Trim();
            string loiMSSV;
            if (!KiemTraMSSVTheoLop(mssvMoi, cboLop.Text.Trim(), out loiMSSV))
            {
                MessageBox.Show(loiMSSV, "MSSV không hợp lệ",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMSSV.Focus();
                return;
            }
            // Kiểm tra CMND gồm đúng 9 chữ số
            if (!System.Text.RegularExpressions.Regex.IsMatch(cmnd, @"^\d{9}$"))
            {
                MessageBox.Show("CMND phải gồm đúng 9 chữ số!");
                txtCMND.Focus();
                return;
            }
            // Kiểm tra số điện thoại gồm đúng 10 chữ số
            string sdt = System.Text.RegularExpressions.Regex.Replace(mtxtSoDT.Text, @"\D", "");
            if (sdt.Length != 10)
            {
                MessageBox.Show("Số điện thoại phải gồm đúng 10 chữ số!");
                mtxtSoDT.Focus();
                return;
            }
            // Tìm sinh viên bằng MSSV ban đầu
            SinhVien sv = qly.DanhSach.FirstOrDefault(x =>
                x.MSSV.Equals(mssvBanDau, StringComparison.OrdinalIgnoreCase));
            if (sv == null)
            {
                MessageBox.Show("Không tìm thấy sinh viên cần cập nhật!");
                return;
            }
            // Kiểm tra MSSV mới có trùng với sinh viên khác không
            bool biTrung = qly.DanhSach.Any(x =>
                !x.MSSV.Equals(mssvBanDau, StringComparison.OrdinalIgnoreCase) &&
                x.MSSV.Equals(mssvMoi, StringComparison.OrdinalIgnoreCase));
            if (biTrung)
            {
                MessageBox.Show("MSSV mới đã tồn tại! Vui lòng nhập MSSV khác.");
                txtMSSV.Focus();
                return;
            }
            // Cập nhật thông tin
            sv.MSSV = mssvMoi;
            sv.HoTenLot = txtHoTenLot.Text.Trim();
            sv.Ten = txtTen.Text.Trim();
            sv.NgaySinh = dtpNgaySinh.Value.Date;
            sv.GioiTinh = rdNam.Checked ? "Nam" : "Nữ";
            sv.Lop = cboLop.Text.Trim();
            sv.CMND = cmnd;
            sv.SDT = sdt;
            sv.DiaChi = txtDiaChi.Text.Trim();
            sv.MonHocDangKy = clbMonHoc.CheckedItems
                .Cast<object>()
                .Select(x => x.ToString())
                .ToList();
            // Lưu và hiển thị lại danh sách
            qly.LuuFile();
            HienThiDanhSach();
            XoaTrangNhapLieu();
            // Cập nhật lại MSSV ban đầu sau khi lưu
            mssvBanDau = mssvMoi;
            MessageBox.Show(
                "Cập nhật thông tin sinh viên thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        private void btnTim_Click(object sender, EventArgs e)
        {
            using (Form formTim = new Form())
            {
                formTim.Text = "Tìm kiếm sinh viên";
                formTim.Width = 390;
                formTim.Height = 245;
                formTim.StartPosition = FormStartPosition.CenterParent;
                formTim.FormBorderStyle = FormBorderStyle.FixedDialog;
                formTim.MaximizeBox = false;
                formTim.MinimizeBox = false;
                Label lblMSSV = new Label();
                lblMSSV.Text = "MSSV:";
                lblMSSV.SetBounds(20, 20, 90, 25);
                TextBox txtMSSVTim = new TextBox();
                txtMSSVTim.SetBounds(110, 18, 240, 25);
                Label lblTen = new Label();
                lblTen.Text = "Tên:";
                lblTen.SetBounds(20, 55, 90, 25);
                TextBox txtTenTim = new TextBox();
                txtTenTim.SetBounds(110, 53, 240, 25);
                Label lblLop = new Label();
                lblLop.Text = "Lớp:";
                lblLop.SetBounds(20, 90, 90, 25);
                TextBox txtLopTim = new TextBox();
                txtLopTim.SetBounds(110, 88, 240, 25);
                Button btnTim = new Button();
                btnTim.Text = "Tìm";
                btnTim.DialogResult = DialogResult.OK;
                btnTim.SetBounds(110, 135, 85, 30);
                Button btnHuy = new Button();
                btnHuy.Text = "Hủy";
                btnHuy.DialogResult = DialogResult.Cancel;
                btnHuy.SetBounds(205, 135, 85, 30);
                formTim.Controls.AddRange(new Control[]
                {lblMSSV, txtMSSVTim,lblTen, txtTenTim,lblLop, txtLopTim,btnTim, btnHuy});
                formTim.AcceptButton = btnTim;
                formTim.CancelButton = btnHuy;
                if (formTim.ShowDialog(this) != DialogResult.OK)
                    return;
                string mssv = txtMSSVTim.Text.Trim();
                string ten = txtTenTim.Text.Trim();
                string lop = txtLopTim.Text.Trim();
                if (string.IsNullOrWhiteSpace(mssv)
                    && string.IsNullOrWhiteSpace(ten)
                    && string.IsNullOrWhiteSpace(lop))
                {
                    MessageBox.Show("Vui lòng nhập ít nhất một điều kiện tìm kiếm!");
                    return;
                }
                var ketQua = qly.DanhSach.Where(sv =>
                    (!string.IsNullOrWhiteSpace(mssv)
                        && (sv.MSSV ?? "").IndexOf(
                            mssv, StringComparison.OrdinalIgnoreCase) >= 0)
                    ||
                    (!string.IsNullOrWhiteSpace(ten)
                        && ((sv.HoTenLot ?? "") + " " + (sv.Ten ?? ""))
                            .IndexOf(
                                ten, StringComparison.OrdinalIgnoreCase) >= 0)
                    ||
                    (!string.IsNullOrWhiteSpace(lop)
                        && (sv.Lop ?? "").IndexOf(
                            lop, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
                dgvSinhVien.DataSource = null;
                dgvSinhVien.DataSource = ketQua;
                if (ketQua.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy sinh viên phù hợp!");
                }
                else
                {
                    MessageBox.Show(
                        "Tìm thấy " + ketQua.Count + " sinh viên.");
                }
            }
        }
        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Xác nhận giá trị CheckBox vừa được chọn
            dgvSinhVien.EndEdit();
            List<string> dsMSSVCanXoa = new List<string>();
            foreach (DataGridViewRow row in dgvSinhVien.Rows)
            {
                if (row.IsNewRow)
                    continue;
                bool daChon =
                    row.Cells["Chon"].Value != null &&
                    Convert.ToBoolean(row.Cells["Chon"].Value);
                if (daChon)
                {
                    string mssv =Convert.ToString(row.Cells[1].Value);
                    if (!string.IsNullOrWhiteSpace(mssv))
                        dsMSSVCanXoa.Add(mssv);
                }
            }
            if (dsMSSVCanXoa.Count == 0)
            {
                MessageBox.Show("Bạn hãy tích chọn ít nhất một sinh viên cần xóa!");
                return;
            }
            DialogResult xacNhan = MessageBox.Show(
                "Bạn có chắc muốn xóa " +
                dsMSSVCanXoa.Count +
                " sinh viên đã chọn không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (xacNhan != DialogResult.Yes)
                return;
            // Xóa khỏi danh sách dữ liệu
            qly.DanhSach.RemoveAll(sv =>dsMSSVCanXoa.Contains(sv.MSSV));
            // Lưu dữ liệu vào tệp
            qly.LuuFile();
            // Hiển thị lại danh sách
            HienThiDanhSach();
            XoaTrangNhapLieu();
            MessageBox.Show("Đã xóa sinh viên được chọn thành công!");
        }
        private void mnuThemMon_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.CurrentRow == null ||dgvSinhVien.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn sinh viên cần thêm môn học!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            SinhVien sv = dgvSinhVien.CurrentRow.DataBoundItem as SinhVien;
            if (sv == null)
                return;
            List<string> danhSachMon = new List<string>();
            foreach (object item in clbMonHoc.Items)
            {
                string tenMon = item.ToString().Trim();
                if (tenMon != "")
                    danhSachMon.Add(tenMon);
            }
            using (Form frm = new Form())
            using (CheckedListBox clb = new CheckedListBox())
            using (Button btnOK = new Button())
            using (Button btnHuy = new Button())
            {
                frm.Text = "Chọn môn học cần thêm";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Size = new Size(350, 400);
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                clb.Dock = DockStyle.Top;
                clb.Height = 300;
                clb.CheckOnClick = true;
                foreach (string mon in danhSachMon)
                {
                    bool daDangKy = sv.MonHocDangKy != null &&
                        sv.MonHocDangKy.Any(m =>
                            string.Equals(m.Trim(),mon,StringComparison.OrdinalIgnoreCase));
                    clb.Items.Add(mon, daDangKy);
                }
                btnOK.Text = "Thêm môn học";
                btnOK.SetBounds(35, 315, 125, 30);
                btnOK.DialogResult = DialogResult.OK;
                btnHuy.Text = "Hủy";
                btnHuy.SetBounds(180, 315, 90, 30);
                btnHuy.DialogResult = DialogResult.Cancel;
                frm.Controls.Add(clb);
                frm.Controls.Add(btnOK);
                frm.Controls.Add(btnHuy);
                frm.AcceptButton = btnOK;
                frm.CancelButton = btnHuy;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    if (sv.MonHocDangKy == null)
                        sv.MonHocDangKy = new List<string>();
                    int soMonThem = 0;
                    foreach (object item in clb.CheckedItems)
                    {
                        string mon = item.ToString();
                        bool daCo = sv.MonHocDangKy.Any(m =>
                            string.Equals(m.Trim(),mon,StringComparison.OrdinalIgnoreCase));
                        if (!daCo)
                        {
                            sv.MonHocDangKy.Add(mon);
                            soMonThem++;
                        }
                    }
                    if (soMonThem == 0)
                    {
                        MessageBox.Show("Không có môn học mới được thêm.");
                        return;
                    }
                    qly.LuuFile();
                    HienThiDanhSach();
                    XoaTrangNhapLieu();
                    MessageBox.Show("Đã thêm " + soMonThem + " môn học cho sinh viên " +
                        sv.MSSV + " thành công!","Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
        private void mnuXoaMon_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.CurrentRow == null ||dgvSinhVien.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần xóa môn học!");
                return;
            }
            SinhVien sv = dgvSinhVien.CurrentRow.DataBoundItem as SinhVien;
            if (sv == null)
                return;
            if (sv.MonHocDangKy == null || sv.MonHocDangKy.Count == 0)
            {
                MessageBox.Show("Sinh viên này chưa đăng ký môn học nào!");
                return;
            }
            using (Form frm = new Form())
            using (CheckedListBox clb = new CheckedListBox())
            using (Button btnXoa = new Button())
            using (Button btnHuy = new Button())
            {
                frm.Text = "Chọn môn học cần xóa";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Size = new Size(350, 400);
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                clb.Dock = DockStyle.Top;
                clb.Height = 300;
                clb.CheckOnClick = true;
                foreach (string mon in sv.MonHocDangKy)
                {
                    clb.Items.Add(mon);
                }
                btnXoa.Text = "Xóa môn đã chọn";
                btnXoa.SetBounds(25, 315, 140, 30);
                btnXoa.DialogResult = DialogResult.OK;
                btnHuy.Text = "Hủy";
                btnHuy.SetBounds(190, 315, 90, 30);
                btnHuy.DialogResult = DialogResult.Cancel;
                frm.Controls.Add(clb);
                frm.Controls.Add(btnXoa);
                frm.Controls.Add(btnHuy);
                frm.AcceptButton = btnXoa;
                frm.CancelButton = btnHuy;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    if (clb.CheckedItems.Count == 0)
                    {
                        MessageBox.Show("Bạn chưa chọn môn học cần xóa!");
                        return;
                    }
                    DialogResult xacNhan = MessageBox.Show("Bạn có chắc chắn muốn xóa " +
                        clb.CheckedItems.Count + " môn học đã chọn?","Xác nhận xóa",
                        MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                    if (xacNhan != DialogResult.Yes)
                        return;
                    List<string> monCanXoa = new List<string>();
                    foreach (object item in clb.CheckedItems)
                    {
                        monCanXoa.Add(item.ToString());
                    }
                    foreach (string mon in monCanXoa)
                    {
                        sv.MonHocDangKy.Remove(mon);
                    }
                    qly.LuuFile();
                    HienThiDanhSach();
                    XoaTrangNhapLieu();
                    MessageBox.Show("Đã xóa môn học thành công!","Thông báo",
                        MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
            }
        }
        private void mnuXoaSinhVien_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.CurrentRow == null ||dgvSinhVien.CurrentRow.DataBoundItem == null)
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần xóa!","Thông báo",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            SinhVien sv = dgvSinhVien.CurrentRow.DataBoundItem as SinhVien;
            if (sv == null)
                return;
            DialogResult xacNhan = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa sinh viên:\n\n" +
                "MSSV: " + sv.MSSV + "\n" +
                "Họ tên: " + sv.HoTenLot + " " + sv.Ten + "?",
                "Xác nhận xóa sinh viên",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (xacNhan != DialogResult.Yes)
                return;
            qly.DanhSach.Remove(sv);
            qly.LuuFile();
            HienThiDanhSach();
            XoaTrangNhapLieu();
            // Xóa thông tin sinh viên khỏi các ô nhập
            txtMSSV.Clear();
            txtHoTenLot.Clear();
            txtTen.Clear();
            txtCMND.Clear();
            txtDiaChi.Clear();
            mtxtSoDT.Clear();
            rdNam.Checked = false;
            rdNu.Checked = false;
            cboLop.SelectedIndex = -1;
            for (int i = 0; i < clbMonHoc.Items.Count; i++)
            {
                clbMonHoc.SetItemChecked(i, false);
            }
            mssvBanDau = null;
            MessageBox.Show("Đã xóa sinh viên thành công!","Thông báo",
                MessageBoxButtons.OK,MessageBoxIcon.Information);
        }
        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult ketQua = MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình không?",
            "Xác nhận thoát",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (ketQua == DialogResult.Yes)
            {
                this.Close();
            }
        }
        private void btnDocTep_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog hopThoai = new OpenFileDialog())
            {
                hopThoai.Title = "Chọn tệp danh sách sinh viên";
                hopThoai.Filter =
                    "Tất cả các tệp được hỗ trợ (*.txt;*.xml;*.json)|*.txt;*.xml;*.json|" +
                    "Tệp văn bản (*.txt)|*.txt|" +
                    "Tệp XML (*.xml)|*.xml|" +
                    "Tệp JSON (*.json)|*.json";
                hopThoai.FilterIndex = 2;
                hopThoai.CheckFileExists = true;
                hopThoai.Multiselect = false;
                if (hopThoai.ShowDialog() != DialogResult.OK)
                    return;
                try
                {
                    string dinhDang = System.IO.Path.GetExtension(hopThoai.FileName).ToLowerInvariant();
                    qly.DocFileTuDuongDan(hopThoai.FileName, dinhDang);
                    HienThiDanhSach();
                    MessageBox.Show("Đọc tệp thành công!","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể đọc tệp!\nChi tiết: " + ex.Message,
                        "Lỗi",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}