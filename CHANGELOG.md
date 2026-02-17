# Changelog

Semua perubahan penting pada proyek ini akan didokumentasikan dalam file ini.

## [1.0.0] - 2026-02-17
### Added
- Inisialisasi proyek ASP.NET Core Web API (.NET 8).
- Konfigurasi **Entity Framework Core** dengan SQL Server.
- Sistem **Autentikasi & Otorisasi** menggunakan JWT (JSON Web Token).
- Migrasi database awal (Users, Rooms, Bookings tables).
- **CRUD Rooms API**: Endpoint untuk mengelola data ruangan.
- **Bookings API**: Endpoint untuk mengajukan dan menyetujui peminjaman.
- Dokumentasi API otomatis menggunakan **Swagger UI**.
- Seed data untuk akun Admin dan Dummy Rooms.

### Security
- Password hashing untuk keamanan akun pengguna.
- Role-based access control (Pemisahan akses Admin vs User).