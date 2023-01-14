using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Post.Query.Domain.Entities
{
    // this links the entity to the table in the database
    // The schema is usefull if you have another user in the appsettings.json other than SA, if you want to preserve the dbo.schema when creating tables.
    [Table("Post", Schema = "dbo")]
    public class PostEntity
    {
        [Key]
        public Guid PostId { get; set; }
        public string? Author { get; set; }
        public DateTime DatePosted { get; set; }
        public string? Message { get; set; }
        public int Likes { get; set; }
        // with this navigation property, when we delete a post, we always delete the comments.
        public virtual ICollection<CommentEntity>? Comments { get; set; }
    }
}