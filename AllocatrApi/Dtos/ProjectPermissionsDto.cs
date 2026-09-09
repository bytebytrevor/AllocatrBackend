namespace AllocatrApi.Dtos;

public record ProjectPermissionsDto(
    bool IsOwner,
    bool IsAcceptedAllocat,
    bool CanManageTasks
);