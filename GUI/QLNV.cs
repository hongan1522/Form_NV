﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DTO;
using BLL;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public partial class QLNV : UserControl
    {
        private readonly QuanLy_BLL bllNV;
        private List<NhanVien>? listNV;

        public QLNV()
        {
            InitializeComponent();
            bllNV = new QuanLy_BLL();
            dgvNV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNV.AutoGenerateColumns = false;
            LoadDataFromJsonFile("C:\\Users\\MyPC\\Desktop\\Form_NV\\DTO\\Employee.json");
            LoadListNV();
        }
        private void QLNV_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(1271, 855);
        }
        private void LoadDataFromJsonFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);

                List<NhanVien> listNVMau = JsonConvert.DeserializeObject<List<NhanVien>>(jsonContent);

                foreach (NhanVien nhanVienMau in listNVMau)
                {
                    dgvNV.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    bllNV.AddNV(nhanVienMau);
                }

                LoadListNV();
                Clear();
            }
            else
            {
                MessageBox.Show("Không tìm thấy file.");
            }
        }
        private void SaveDataToJsonFile(string filePath, List<NhanVien> listNhanVien)
        {
            string jsonContent = JsonConvert.SerializeObject(listNhanVien, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);
        }
        private void SaveDataAndRefreshGrid()
        {
            SaveDataToJsonFile("C:\\Users\\MyPC\\Desktop\\Form_NV\\DTO\\Employee.json", listNV);
        }
        private void LoadListNV()
        {
            listNV = bllNV.GetListNV();
            dgvNV.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvNV.DataSource = null;
            dgvNV.DataSource = listNV;
        }
        private void Clear()
        {
            txtMaNV.Enabled = true;
            txtMaNV.Clear();
            txtTenNV.Clear();
            dtpNS.Value = DateTime.Now;
            txtEmail.Clear();
            txtSDT.Clear();
            txtDC.Clear();
        }
        private bool TextBoxesFilled()
        {
            return !string.IsNullOrWhiteSpace(txtMaNV.Text) &&
                   !string.IsNullOrWhiteSpace(txtTenNV.Text) &&
                   !string.IsNullOrWhiteSpace(txtDC.Text) &&
                   !string.IsNullOrWhiteSpace(txtSDT.Text) &&
                   !string.IsNullOrWhiteSpace(txtEmail.Text);
        }
        public bool IsMaNVValidAndNotDuplicate(NhanVien nhanVien)
        {
            // Kiểm tra Mã nhân viên không được null hoặc rỗng
            if (string.IsNullOrEmpty(nhanVien.MaNhanVien))
            {
                MessageBox.Show("Mã nhân viên không được rỗng.");
                return false;
            }

            // Kiểm tra xem MaNV có đúng định dạng không
            Regex regex = new(@"^NV\d+$");
            if (!regex.IsMatch(nhanVien.MaNhanVien))
            {
                MessageBox.Show("Mã nhân viên không hợp lệ. Vui lòng nhập đúng định dạng (vd: NV1, NV2,....)");
                return false;
            }

            // Kiểm tra xem MaNV đã tồn tại trong danh sách nhân viên chưa
            foreach (NhanVien nv in listNV)
            {
                if (nv.MaNhanVien == nhanVien.MaNhanVien)
                {
                    return false;
                }
            }

            return true;
        }
        public bool IsNVValid(NhanVien nv)
        {
            // Kiểm tra null hoặc không phải chuỗi số
            if (string.IsNullOrEmpty(nv.SDT) || !nv.SDT.All(char.IsDigit))
            {
                return false;
            }

            // Kiểm tra bắt đầu từ số 0 và có đúng 10 chữ số
            if (!(nv.SDT.Length == 10 && nv.SDT.StartsWith("0")))
            {
                return false;
            }

            // Kiểm tra email hợp lệ
            try
            {
                var addr = new System.Net.Mail.MailAddress(nv.Email);
                if (addr.Address != nv.Email)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            // Kiểm tra ngày sinh
            if (nv.NgaySinh >= DateTime.Now)
            {
                return false;
            }

            return true;
        }
        public bool IsPhoneNumberDuplicate(NhanVien nv)
        {
            foreach (NhanVien existingNV in listNV)
            {
                if (existingNV.SDT == nv.SDT)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsEmailDuplicate(NhanVien nv)
        {
            foreach (NhanVien existingNV in listNV)
            {
                if (existingNV.Email == nv.Email)
                {
                    return true;
                }
            }
            return false;
        }
        private string AutoGenerateMaNV()
        {
            // Lấy danh sách mã nhân viên hiện có
            List<string> existingMaNVs = listNV.Select(nv => nv.MaNhanVien).ToList();

            // Tìm mã nhân viên lớn nhất
            string maxMaNV = existingMaNVs.OrderByDescending(maNV => int.Parse(maNV.Substring(2))).FirstOrDefault();

            // Tạo mã mới dựa trên mã lớn nhất
            int newMaNVNumber = int.Parse(maxMaNV.Substring(2)) + 1;
            string newMaNV = "NV" + newMaNVNumber.ToString().PadLeft(3, '0');

            return newMaNV;
        }
        private void dgvNV_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewRow dr = dgvNV.CurrentRow;
            if (dr != null)
            {
                txtMaNV.Text = (dr.Cells["MaNV"].Value ?? string.Empty).ToString();
                txtTenNV.Text = (dr.Cells["TenNV"].Value ?? string.Empty).ToString();
                dtpNS.Value = Convert.ToDateTime(dr.Cells["NgaySinh"].Value ?? DateTime.Now);
                txtEmail.Text = (dr.Cells["Email"].Value ?? string.Empty).ToString();
                txtSDT.Text = (dr.Cells["SDT"].Value ?? string.Empty).ToString();
                txtDC.Text = (dr.Cells["DiaChi"].Value ?? string.Empty).ToString();
            }
            txtMaNV.Enabled = false;
        }
        private void QLNV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && TextBoxesFilled())
            {
                e.SuppressKeyPress = true;
                btnNhapNV_Click(sender, e);
            }
        }
        private void dgvNV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < dgvNV.Rows.Count; i++)
            {
                dgvNV.Rows[i].Cells["MaNV"].Value = "NV" + (i + 1);
            }
        }
        private void dgvNV_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Lấy số thứ tự của hàng (bắt đầu từ 1)
            int rowIndex = e.RowIndex + 1;

            // Tạo một brush để vẽ số thứ tự
            using (SolidBrush brush = new SolidBrush(dgvNV.RowHeadersDefaultCellStyle.ForeColor))
            {
                // Chuẩn bị chuỗi số thứ tự
                string rowIndexText = rowIndex.ToString();

                // Xác định vị trí để vẽ số thứ tự trên hàng tiêu đề
                float x = e.RowBounds.Left + 20;
                float y = e.RowBounds.Top + (e.RowBounds.Height - e.InheritedRowStyle.Font.Height) / 2;

                // Vẽ số thứ tự
                e.Graphics.DrawString(rowIndex.ToString(), e.InheritedRowStyle.Font, brush, x, y);
            }
        }
        private void btnNhapNV_Click(object sender, EventArgs e)
        {
            dtpNS.Format = DateTimePickerFormat.Custom;
            dtpNS.CustomFormat = "dd/MM/yyyy";

            NhanVien nv = new NhanVien()
            {
                MaNhanVien = txtMaNV.Text,
                TenNhanVien = txtTenNV.Text,
                DiaChi = txtDC.Text,
                SDT = txtSDT.Text,
                Email = txtEmail.Text,
                NgaySinh = dtpNS.Value.Date
            };

            List<string> errorMessages = new List<string>();

            if (!IsNVValid(nv))
            {
                errorMessages.Add("_ Nhân viên không hợp lệ (Định dạng email: name@gmail.com, SĐT bắt đầu từ 0 và có 10 chữ số, ngày sinh không thể chọn ngày hiện tại).\n");
            }

            if (!IsMaNVValidAndNotDuplicate(nv))
            {
                nv.MaNhanVien = AutoGenerateMaNV();
                //errorMessages.Add("_ Mã nhân viên không hợp lệ hoặc trùng lặp.\n");
            }

            if (IsEmailDuplicate(nv))
            {
                errorMessages.Add("_ Email đã tồn tại.\n");
            }

            if (IsPhoneNumberDuplicate(nv))
            {
                errorMessages.Add("_ Số điện thoại đã tồn tại.");
            }

            if (errorMessages.Count > 0)
            {
                string errorMessage = string.Join("\n", errorMessages);
                MessageBox.Show("Thêm thất bại:\n" + errorMessage);
            }
            else
            {
                bllNV.AddNV(nv);
                LoadListNV();
                Clear();
                MessageBox.Show("Thêm thành công");
            }
        }
        private void btnXoaNV_Click(object sender, EventArgs e)
        {
            if (dgvNV.SelectedCells.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa.");
                return;
            }

            List<string> selectedMaNVs = new List<string>();

            foreach (DataGridViewRow selectedRow in dgvNV.SelectedRows)
            {
                string maNV = (selectedRow.Cells["MaNV"].Value ?? string.Empty).ToString();
                if (!string.IsNullOrEmpty(maNV))
                {
                    selectedMaNVs.Add(maNV);
                }
            }

            if (selectedMaNVs.Count > 0)
            {
                string selectedMaNVsText = string.Join(", ", selectedMaNVs);

                DialogResult dialogResult = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên có danh sách mã {selectedMaNVsText} không?", "Xác nhận xóa?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        foreach (string maNV in selectedMaNVs)
                        {
                            bllNV.RemoveNV(maNV);
                            listNV.Remove(listNV.Find(nv => nv.MaNhanVien == maNV));
                        }

                        LoadListNV();
                        MessageBox.Show($"Xóa nhân viên thành công. Đã xóa danh sách mã {selectedMaNVsText}.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Xóa nhân viên không thành công. Lỗi: " + ex.Message);
                    }
                }
            }

            // Cập nhật lại mã nhân viên
            for (int i = 0; i < listNV.Count; i++)
            {
                listNV[i].MaNhanVien = "NV" + (i + 1).ToString();
            }
        }

        private bool isEditing = false;

        private void btnSuaNV_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                isEditing = true;
                txtMaNV.Enabled = false;

                if (isEditing)
                {
                    // Lấy dữ liệu từ các trường
                    string maNhanVien = txtMaNV.Text;
                    string tenNhanVien = txtTenNV.Text;
                    DateTime ngaySinh = dtpNS.Value;
                    string email = txtEmail.Text;
                    string sdt = txtSDT.Text;
                    string diaChi = txtDC.Text;

                    // Tạo đối tượng NhanVien từ dữ liệu vừa lấy
                    NhanVien nv = new NhanVien()
                    {
                        MaNhanVien = maNhanVien,
                        TenNhanVien = tenNhanVien,
                        NgaySinh = ngaySinh,
                        Email = email,
                        SDT = sdt,
                        DiaChi = diaChi
                    };

                    if (IsNVValid(nv))
                    {
                        bllNV.UpdateNV(nv); // Cập nhật dữ liệu vào CSDL
                        LoadListNV(); // Nạp lại danh sách nhân viên
                        Clear();

                        MessageBox.Show("Cập nhật thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật không thành công. Vui lòng kiểm tra lại thông tin.");
                    }

                    isEditing = false; // Chuyển trạng thái về là không sửa đổi
                    txtMaNV.Enabled = true;
                }
            }
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (int i = 0; i < dgvNV.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = dgvNV.Columns[i].HeaderText;
                }

                try
                {
                    for (int i = 0; i < dgvNV.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgvNV.Columns.Count; j++)
                        {
                            DateTime NgaySinhValue = (DateTime)dgvNV.Rows[i].Cells[2].Value;
                            string NgaySinhFormated = NgaySinhValue.ToString("dd/MM/yyyy");

                            worksheet.Cells[i + 2, 3].Value = NgaySinhFormated;
                            worksheet.Cells[i + 2, j + 1].Value = dgvNV.Rows[i].Cells[j].Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx|All Files|*.*",
                    FileName = "exportNV.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var file = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(file);
                    MessageBox.Show("Xuất dữ liệu thành công.");
                }
            }
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        int row = 2;
                        int maxMaNV = 0;

                        while (worksheet.Cells[row, 1].Value != null)
                        {
                            int maNhanVien;
                            if (int.TryParse(worksheet.Cells[row, 1].Value.ToString().Replace("NV", ""), out maNhanVien))
                            {
                                NhanVien nv = new NhanVien
                                {
                                    MaNhanVien = worksheet.Cells[row, 1].Value.ToString(),
                                    TenNhanVien = worksheet.Cells[row, 2].Value.ToString(),
                                    NgaySinh = DateTime.ParseExact(worksheet.Cells[row, 3].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    Email = worksheet.Cells[row, 4].Value.ToString(),
                                    SDT = worksheet.Cells[row, 5].Value.ToString(),
                                    DiaChi = worksheet.Cells[row, 6].Value.ToString()
                                };

                                if (IsMaNVValidAndNotDuplicate(nv))
                                {
                                    listNV.Add(nv);
                                    maxMaNV = Math.Max(maxMaNV, maNhanVien);
                                }
                            }
                            row++;
                        }

                        dgvNV.DataSource = null;
                        dgvNV.DataSource = listNV;
                        dgvNV.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Clear();
        }
    }
}
