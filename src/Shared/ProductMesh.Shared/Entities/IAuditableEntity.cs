namespace ProductMesh.Shared.Entities;

public interface IAuditableEntity
{
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
