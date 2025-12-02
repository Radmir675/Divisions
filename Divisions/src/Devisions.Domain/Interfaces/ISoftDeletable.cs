using System;

namespace Devisions.Domain.Interfaces;

public interface ISoftDeletable
{
    void SoftDelete(DateTime? deletedAt = null);

    void Restore();
}