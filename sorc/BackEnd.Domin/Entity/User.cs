using BackEnd.Domin.Entity;
using BackEnd.Domin.Entity.Enums;
using BackEnd.Domin.ValueObjects;
using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public Guid Id { get; private set; }
    public Guid AddressId { get; private set; }

    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Password Password { get; private set; }
    public ProfileImage? Profile { get; private set; }
    public RoleType Role { get; private set; } = RoleType.user;
    public bool IsEmailVerified { get; private set; } = false;

    [ForeignKey(nameof(AddressId))]
    public Address Address { get; private set; }

    protected User() { }

    private User(Guid addressId, UserName name, Email email, PhoneNumber phone, Password password,
        ProfileImage? profile, RoleType role)
    {
        Id = Guid.NewGuid();
        AddressId = addressId;
        Name = name;
        Email = email;
        Phone = phone;
        Password = password;
        Profile = profile;
        Role = role;
    }

    public static User Create(Guid addressId, UserName name, Email email, PhoneNumber phone, Password password,
        ProfileImage? profile = null, RoleType role = RoleType.user)
        => new User(addressId, name, email, phone, password, profile, role);

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new InvalidOperationException("Email is already verified.");
        IsEmailVerified = true;
    }
    public void ChangePassword(Password newPassword)
    {
        if (newPassword == null)
            throw new ArgumentNullException(nameof(newPassword));

        // لو أردت منع إعادة نفس الباسورد القديم:
        if (newPassword.Equals(Password))
            throw new InvalidOperationException("New password must be different from the current one.");

        Password = newPassword;
    }

}
