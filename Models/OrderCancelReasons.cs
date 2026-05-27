using System.Text;

namespace EcommerceApi.Models
{
    /// <summary>Danh sách lý do hủy đơn — user chọn khi gửi yêu cầu hủy.</summary>
    public static class OrderCancelReasons
    {        public static readonly string[] All =
        [
            "Đặt nhầm sản phẩm",
            "Muốn thay đổi sản phẩm khác",
            "Thay đổi số lượng sản phẩm",
            "Không còn nhu cầu mua hàng",
            "Tìm được giá tốt hơn ở nơi khác",
            "Thời gian giao hàng quá lâu",
            "Sai thông tin đơn hàng",
            "Muốn thay đổi địa chỉ nhận hàng",
            "Muốn thay đổi phương thức thanh toán",
            "Sản phẩm không đúng nhu cầu",
            "Đặt trùng đơn hàng",
            "Không đủ khả năng thanh toán",
            "Lỗi khi đặt hàng",
            "Không liên lạc được với người bán",
            "Muốn đặt lại đơn mới",
            "Hết hàng / không còn sản phẩm",
            "Lý do cá nhân",
            "Khác"
        ];

        public static string Normalize(string? reason) =>
            string.IsNullOrWhiteSpace(reason)
                ? string.Empty
                : reason.Trim().Normalize(NormalizationForm.FormC);

        public static bool IsValid(string? reason)
        {
            var key = Normalize(reason);
            if (string.IsNullOrEmpty(key)) return false;
            return All.Any(x => Normalize(x) == key);
        }

        public static string? Resolve(string? reason)
        {
            var key = Normalize(reason);
            if (string.IsNullOrEmpty(key)) return null;
            return All.FirstOrDefault(x => Normalize(x) == key);
        }
    }
}