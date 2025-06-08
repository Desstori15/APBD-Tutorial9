using APBD_Tutorial9.Model;

namespace APBD_Tutorial9.Services;



    public interface IWarehouseService
    {
        Task<int> AddProductManually(ProductWarehouseRequest request);
        Task<int> AddProductWithProcedure(ProductWarehouseRequest request);
    }
