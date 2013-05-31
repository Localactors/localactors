using System;
using System.ComponentModel.DataAnnotations;

namespace Localactors.entities
{
    [MetadataType(typeof(project))]
    public class project_MetaData
    {
        public int ProjectID;
        [Required]
        public int UserID;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Text)]
        public string Title;
        [Required]
        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateEnd;
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateStart;

        [Required]
        public int CountryID;

        [DataType(DataType.MultilineText)]
        public string Location;
    }

    [MetadataType(typeof(user))]
    public class user_MetaData
    {
        public int UserID;

        [Required]
        [MaxLength(10)]
        [DataType(DataType.Text)]
        public string Role;

        [Required]
        [MaxLength(20)]
        [DataType(DataType.Text)]
        public string UserName;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string UserPassword;

        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string Name;
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string Lastname;

        [MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Bio;

        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email;

        [Required]
        public int CountryID;


    }

    [MetadataType(typeof(user_roles))]
    public class roles_MetaData
    {
        public int RoleID;

        [Required]
        [MaxLength(10)]
        [DataType(DataType.Text)]
        public string RoleName;
    }


    [MetadataType(typeof(tag))]
    public class tag_MetaData
    {
        public int TagID;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Text)]
        public string Name;
    }

    [MetadataType(typeof(achievement))]
    public class achievement_MetaData
    {
        public int AchievementID;

        [Required]
        public int ProjectID;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Text)]
        public string Title;

        [DataType(DataType.MultilineText)]
        public string Motivation;

        [DataType(DataType.MultilineText)]
        public string Goal;
    }

    [MetadataType(typeof(donation))]
    public class donation_MetaData
    {
        public int InvestmentID;

        [Required]
        public int UserID;

        public int ProjectID;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Currency)]
        public Decimal Amount;

        [DataType(DataType.MultilineText)]
        public string Description;
    }


    [MetadataType(typeof(project_guestbook))]
    public class guestbook_MetaData
    {
        public int GuestpostID;

        [Required]
        public int UserID;

        [Required]
        public int ProjectID;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;

        [DataType(DataType.MultilineText)]
        public string Text;

        [MaxLength(255)]
        [DataType(DataType.ImageUrl)]
        public string Picture;
    }


    [MetadataType(typeof(update))]
    public class update_MetaData
    {
        public int UpdateID;

        [Required]
        public int ProjectID;

        [MaxLength(255)]
        [DataType(DataType.Text)]
        public string Title;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;

    }

    [MetadataType(typeof(update_comment))]
    public class update_comment_MetaData
    {
        public int CommentID;

        [Required]
        public int UpdateID;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date;

        [DataType(DataType.MultilineText)]
        public string Text;

        [MaxLength(255)]
        [DataType(DataType.ImageUrl)]
        public string Picture;

    }


    [MetadataType(typeof(update_content))]
    public class update_content_MetaData
    {
        public int ContentID;

        [Required]
        public int UpdateID;

        [Required]
        public int ContentTypeID;

        [Required]
        public int Order;



    }

    [MetadataType(typeof(update_content_type))]
    public class update_content_type_MetaData
    {
        public int ContentTypeID;

        [Required]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string ContentTypeName;
    }

    [MetadataType(typeof(country))]
    public class country_MetaData
    {
        public int CountryID;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Text)]
        public string Name;

        [Required]
        [MaxLength(5)]
        [DataType(DataType.Text)]
        public string Code;
    }

}



