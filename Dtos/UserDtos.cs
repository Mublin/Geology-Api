namespace Geology_Api.Dtos;

public record CreateUserDto(string Email, string RegistrationNumber, string Hash);
public record AddUserDto(string RegistrationNumber, string Name);
public record UserDto(string Email, string RegistrationNumber, string Name, bool IsStudent, bool IsLecturer, bool IsAdmin, bool IsActivated, int UserId, DateTime? DateCreated, DateTime? DateUpdated, string Token);
public record UserDtoList(string Email, string RegistrationNumber, string Name, bool IsStudent, bool IsLecturer, bool IsAdmin, bool IsActivated, int UserId, DateTime? DateCreated, DateTime? DateUpdated);


public record LogInDto(string RegistrationNumber, string Hash);

public record UpdatePasswordDto( string Hash, int UserId, string OldHash);
public record UpdateAdminDto(bool Admin, int UserId);

public record UpdateInfoDto(int UserId, string Email, string Name);


public record UserToDto(int UserId, string Hash,  DateTime DateCreated, DateTime? DateUpdated, int Level);
