﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Orders
    {
        public string MaTrangThaiDonHang { get; set; } = null!;
        public string TenTrangThai { get; set; } = null!;
        public string? MoTa { get; set; }

        public Orders() { }

        public Orders(string maTrangThaiDonHang, string tenTrangThai, string? moTa)
        {
            MaTrangThaiDonHang = maTrangThaiDonHang;
            TenTrangThai = tenTrangThai;
            MoTa = moTa;
        }
    }
    
}
