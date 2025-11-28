namespace Devisions.Domain.Interfaces;

public interface ISoftDeletable
{
    void SoftDelete();

    void Restore();
}