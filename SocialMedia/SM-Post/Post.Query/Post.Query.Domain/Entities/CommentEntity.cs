using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Post.Query.Domain.Entities
{
    // this links the entity to the table in the database
    // The schema is usefull if you have another user in the appsettings.json other than SA, if you want to preserve the dbo.schema when creating tables.
    [Table("Comment", Schema = "dbo")] 
    public class CommentEntity
    {
        [Key]
        public Guid CommentId { get; set; }
        public string? Username { get; set; }
        public DateTime CommentDate { get; set; }
        public string? Comment { get; set; }
        public bool Edited { get; set; }
        public Guid PostId { get; set; }

        [JsonIgnore]
        public virtual PostEntity? Post { get; set; }
        // CommentEntity is referenced by PostEntity, so, without the jsonignore we have circular reference.
        // but we also want the PostEntity inside the commentEntity.
    }
}