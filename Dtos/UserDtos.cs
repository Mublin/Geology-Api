namespace Geology_Api.Dtos;

public record CreateUserDto(string Email, string RegistrationNumber, string Hash);
public record AddUserDto(string RegistrationNumber, string Name);
public record UserDto(string Email, string RegistrationNumber, string Name, bool IsStudent, bool IsLecturer, bool IsAdmin, bool IsActivated, int Id, DateTime? DateCreated, DateTime? DateUpdated, string AccessToken);
public record UserWithTokenDto(string Email, string RegistrationNumber, string Name, bool IsStudent, bool IsLecturer, bool IsAdmin, bool IsActivated, bool IsSuperAdmin, int Id, DateTime? DateCreated, DateTime? DateUpdated, string AccessToken);

public record UserDtoList(string Email, string RegistrationNumber, string Name, bool IsStudent, bool IsLecturer, bool IsAdmin, bool IsActivated, int Id, DateTime? DateCreated, DateTime? DateUpdated);

public record TokenRequest(string RefreshToken);
public record GetUserSuperAdmin(string RegNo);

public record LogInDto(string RegistrationNumber, string Hash);

public record UpdatePasswordDto( string Hash, int Id, string OldHash);
public record UpdateAdminDto(bool Admin, int Id);

public record UpdateInfoDto(int Id, string Email, string Name);


public record UserToDto(int Id, string Hash,  DateTime DateCreated, DateTime? DateUpdated, int Level);
