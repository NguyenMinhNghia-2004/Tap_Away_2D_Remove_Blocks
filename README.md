# Sonat Intern Test 2 - Tap away 2D: Remove blocks

Dự án này là một bài kiểm tra/thử nghiệm (Intern Test) phát triển trên Unity, tập trung vào việc tạo 1 bản prototype gameplay của game “Tap away 2D: Remove blocks”.

## 🎮 Demo

### Hình ảnh Gameplay

![Ảnh chụp màn hình 1](https://drive.google.com/uc?export=view&id=1iBTeaSF8zX6jdcKLcir9BUTCvW7a-ApZ)

![Ảnh chụp màn hình 2](https://drive.google.com/uc?export=view&id=1YF_ksaviqaEyTCy0YEx-eWSUZtkDy2DE)

![Ảnh chụp màn hình 3](https://drive.google.com/uc?export=view&id=1BxjKQKKSf882XeYXLGFiSPOlF31NCdeH)

![Ảnh chụp màn hình 4](https://drive.google.com/uc?export=view&id=16-Eoe2UFd8izPcNuYSUy_jAPZsQTW8NY)

![Ảnh chụp màn hình 5](https://drive.google.com/uc?export=view&id=1BjNsmf2YOdnjRvUL3SfRjY_T_u1fE5p1)

### Video Demo

[![Xem Video Demo](https://drive.google.com/uc?export=view&id=1_GgT1y6-JUF3Y2_n-yzl-4-Hxl3D8-lF)](https://drive.google.com/file/d/1fRsbHVG42iYqMrN1M1DMk3a4LP7LY3Md/view?usp=drive_link)
*(Click vào ảnh trên để xem video Demo)*
---

## 🌟 Tính năng chính (Key Features)

Dựa trên quá trình phát triển, dự án bao gồm các cơ chế chính sau:
- **Cơ chế chơi**: Dọn các block ra khỏi màn chơi trong số lượt cho phép.
- **Cơ chế của khối (Block)**: Các khối có thể di chuyển theo hướng mũi tên của nó, có thể bị cản, phá hủy bởi vật cản và xoay quanh Block Rotation.
- **Cơ chế Xoay khối (Block Rotation)**: Người chơi có thể tương tác với `TurnObject` để xoay các khối xung quanh nó.
- **Tính toán Vị trí chính xác**: Đảm bảo các khối (Blocks) rơi và nằm chính xác trên hệ thống lưới (Grid) sau khi hoàn thành thao tác.
- **Hiệu ứng Animation mượt mà (Animations)**: Các hiệu ứng di chuyển của các khối và UI diễn ra mượt mà tăng trải nghiệm người chơi game.
- **Quản lý Lượt chơi**: Mỗi thao tác xoay được tính là một lượt di chuyển (Move), đồng thời có hệ thống ngăn chặn thao tác của người chơi liên tục cho đến khi hiệu ứng hoàn tất.
- **Âm thanh (Sound)**: Có hệ thống âm thanh cho các thao tác như di chuyển, xoay, phá hủy, và âm thanh nền.
- **UI (User Interface)**: Có hệ thống UI hiển thị số lượt chơi còn lại, số lượt đã sử dụng, và các nút điều khiển như Setting, Reset.
- **Quản lý màn chơi (Level Management)**: Có hệ thống quản lý màn chơi, bao gồm cả việc lưu và tải trạng thái màn chơi.

## 🛠️ Công nghệ / Công cụ sử dụng

- **Game Engine:** Unity (6000.0.55f1)
- **Ngôn ngữ lập trình:** C#
- **Môi trường:** 2D

## 📁 Cấu trúc Code chính

Một số script chính nắm giữ logic của trò chơi nằm trong thư mục `Assets/Scripts`:

- **`TurnObject.cs`**: Xử lý logic của đối tượng trung tâm dùng để xoay, bao gồm cả hiệu ứng hiển thị (icon animation) và xác định tính hợp lệ của thao tác.
- **`Block.cs`**: Quản lý trạng thái và vị trí của các khối có thể bị tác động. Hàm xoay vị trí và đáp xuống trục tọa độ (ví dụ hạ trục Y xuống độ cao phù hợp).
- **`GameManager.cs`**: Quản lý trạng thái của trò chơi, bao gồm cả việc lưu và tải trạng thái màn chơi.
- **`LevelManager.cs`**: Quản lý màn chơi, bao gồm cả việc lưu và tải trạng thái màn chơi.
- **`AudioManager.cs`**: Quản lý âm thanh, bao gồm cả việc phát âm thanh cho các thao tác như di chuyển, xoay, phá hủy, và âm thanh nền.
- **`UIManager.cs`**: Quản lý UI, bao gồm cả việc hiển thị số lượt chơi còn lại, số lượt đã sử dụng, và các nút điều khiển như Setting, Reset.

## 🚀 Hướng dẫn Cài đặt & Chạy dự án

1. **Yêu cầu hệ thống**: Máy tính cần cài đặt sẵn **Unity Hub** và **Unity Editor** (phiên bản tương ứng với project).
2. **Clone hoặc Tải source code**: Tải toàn bộ thư mục dự án `Tap_Away_2D_Remove_Blocks` về máy tính.
3. **Mở dự án**:
   - Mở Unity Hub.
   - Chọn "Add project from disk" (Mở dự án từ ổ cứng).
   - Dẫn đường dẫn tới thư mục ví dụ`d:\Unity\Tap_Away_2D_Remove_Blocks` (hoặc nơi bạn lưu trữ) và chọn "Open".
4. **Chạy Game**:
   - Trong giao diện Unity, mở Scene chính của trò chơi `Assets/Scenes/SampleScene`.
   - Nhấn nút **Play ▷** ở thanh công cụ phía trên để xem và tương tác dự án.
