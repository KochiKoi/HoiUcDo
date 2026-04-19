
# Hồi Ức Đỏ (Red Memories) 🇻🇳
> **Một trò chơi điện tử 2D về lịch sử Việt Nam, lấy cảm hứng từ tinh thần phản chiến và những linh hồn vô danh.**

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.46f1%20LTS-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Windows-blue)](https://www.microsoft.com/windows)

## 📖 Giới thiệu dự án
**Hồi Ức Đỏ** là dự án đồ án tốt nghiệp ngành Công nghệ Thông tin tại trường Đại học Công nghệ TP.HCM (HUTECH). Trò chơi được phát triển với mục tiêu thay đổi cách tiếp cận lịch sử khô khan qua sách vở bằng trải nghiệm tương tác sống động.

Lấy cảm hứng từ tựa game *Valiant Hearts: The Great War*, dự án tái hiện cuộc kháng chiến chống thực dân Pháp (1945 - 1954) dưới góc nhìn của những **nhân vật vô danh**. Họ là những người lính, người dân bình thường từ cả hai phía chiến tuyến, đại diện cho những phận người nhỏ bé bị cuốn vào dòng chảy khốc liệt của chiến tranh.

### Thông điệp cốt lõi:
* **Tính nhân văn:** Khắc họa sự vô danh đầy bi kịch và những mất mát lặng thầm.
* **Hòa bình:** Đề cao giá trị của hòa bình thông qua những câu chuyện cảm xúc.
* **Giáo dục:** Tiếp cận lịch sử một cách sáng tạo, giúp người trẻ dễ dàng thấu hiểu và ghi nhớ.

## 🎮 Lối chơi (Gameplay)
Trò chơi kết hợp giữa phiêu lưu giải đố (Puzzle Adventure) và kể chuyện tuyến tính trên nền đồ họa 2D Side-scrolling.

* **Cơ chế tương tác:** Người chơi di chuyển, tương tác với vật thể (kéo đẩy thùng, gạt cần gạt) để giải quyết các tình huống cản trở trên đường đi.
* **Hệ thống chiến đấu đặc biệt:** Điều khiển các phương tiện cơ giới như xe tăng và pháo binh (bao gồm nâng/hạ nòng pháo và khai hỏa).
* **Vật phẩm thu thập (Collectibles):** Hệ thống tư liệu lịch sử với các vật phẩm có thật như súng Arisaka, bom ba càng, thư tay... giúp người chơi hiểu sâu hơn về bối cảnh thời đại.

## 🛠 Công nghệ & Kỹ thuật
* **Engine:** Unity Engine 2022.3.46f1 (LTS).
* **Ngôn ngữ:** C#.
* **Đồ họa:** 2D vẽ tay phong cách màu nước, kết hợp kỹ thuật đổ bóng tạo chiều sâu.
* **Kỹ thuật nổi bật:**
    * **Parallax Scrolling:** Tạo chiều sâu đa lớp cho môi trường 2D.
    * **Object Pooling:** Tối ưu hóa hiệu suất xử lý các hiệu ứng cháy nổ và đạn dược.
    * **State Machine:** Quản lý các trạng thái phức tạp của nhân vật và AI kẻ địch.

## 💻 Cấu hình yêu cầu
| Thông số | Cấu hình tối thiểu | Cấu hình đề xuất |
| :--- | :--- | :--- |
| **CPU** | Intel Pentium IV 3.0 GHz | Intel Core 2 Duo E4400 2.0 GHz |
| **RAM** | 2 GB | 2 GB |
| **Đồ họa** | NVIDIA GeForce 8800 GT | NVIDIA GeForce 9600 GT |
| **HĐH** | Windows 10 (64-bit) | Windows 10 (64-bit) |

## 📂 Cấu trúc mã nguồn
```text
Assets/
├── Scripts/      # Toàn bộ mã nguồn điều khiển gameplay và hệ thống.
├── Sprites/      # Tài nguyên đồ họa nhân vật, môi trường vẽ tay.
├── Audio/        # Âm thanh môi trường và nhạc nền.
└── Prefabs/      # Các đối tượng mẫu được thiết lập sẵn.
