using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZInventory.Model
{
    public class UnitOfWork : IDisposable
    {
        private EZInventoryEntities context = new EZInventoryEntities();
        private GenericRepository<Item> itemRepository;
        private GenericRepository<Inventory> inventoryRepository;
        private GenericRepository<Order> orderRepository;
        private GenericRepository<OrderDetail> orderDetailRepository;
        private GenericRepository<Company> companyRepository;
        private GenericRepository<Store> storeRepository;
        private GenericRepository<Status> statusRepository;
        private GenericRepository<Location> locationRepository;
        private GenericRepository<Box> boxRepository;
        private GenericRepository<InventoryDetail> inventoryDetailRepository;
        private GenericRepository<CompanyOrder> companyOrderRepository;


        public GenericRepository<Item> ItemRepository
        {
            get
            {

                if (this.itemRepository == null)
                {
                    this.itemRepository = new GenericRepository<Item>(context);
                }
                return itemRepository;
            }
        }

        public GenericRepository<Inventory> InventoryRepository
        {
            get
            {

                if (this.inventoryRepository == null)
                {
                    this.inventoryRepository = new GenericRepository<Inventory>(context);
                }
                return inventoryRepository;
            }
        }

        public GenericRepository<Order> OrderRepository
        {
            get
            {

                if (this.orderRepository == null)
                {
                    this.orderRepository = new GenericRepository<Order>(context);
                }
                return orderRepository;
            }
        }

        public GenericRepository<OrderDetail> OrderDetailRepository
        {
            get
            {

                if (this.orderDetailRepository == null)
                {
                    this.orderDetailRepository = new GenericRepository<OrderDetail>(context);
                }
                return orderDetailRepository;
            }
        }

        public GenericRepository<Company> CompanyRepository
        {
            get
            {

                if (this.companyRepository == null)
                {
                    this.companyRepository = new GenericRepository<Company>(context);
                }
                return companyRepository;
            }
        }

        public GenericRepository<Store> StoreRepository
        {
            get
            {

                if (this.storeRepository == null)
                {
                    this.storeRepository = new GenericRepository<Store>(context);
                }
                return storeRepository;
            }
        }

        public GenericRepository<Status> StatusRepository
        {
            get
            {

                if (this.statusRepository == null)
                {
                    this.statusRepository = new GenericRepository<Status>(context);
                }
                return statusRepository;
            }
        }

        public GenericRepository<Location> LocationRepository
        {
            get
            {

                if (this.locationRepository == null)
                {
                    this.locationRepository = new GenericRepository<Location>(context);
                }
                return locationRepository;
            }
        }

        public GenericRepository<Box> BoxRepository
        {
            get
            {

                if (this.boxRepository == null)
                {
                    this.boxRepository = new GenericRepository<Box>(context);
                }
                return boxRepository;
            }
        }

        public GenericRepository<InventoryDetail> InventoryDetailRepository
        {
            get
            {

                if (this.inventoryDetailRepository == null)
                {
                    this.inventoryDetailRepository = new GenericRepository<InventoryDetail>(context);
                }
                return inventoryDetailRepository;
            }
        }

        public GenericRepository<CompanyOrder> CompanyOrderRepository
        {
            get
            {

                if (this.companyOrderRepository == null)
                {
                    this.companyOrderRepository = new GenericRepository<CompanyOrder>(context);
                }
                return companyOrderRepository;
            }
        }
        //private SchoolContext context = new SchoolContext();
        //private GenericRepository<Department> departmentRepository;
        //private GenericRepository<Course> courseRepository;

        //public GenericRepository<Department> DepartmentRepository
        //{
        //    get
        //    {

        //        if (this.departmentRepository == null)
        //        {
        //            this.departmentRepository = new GenericRepository<Department>(context);
        //        }
        //        return departmentRepository;
        //    }
        //}

        //public GenericRepository<Course> CourseRepository
        //{
        //    get
        //    {

        //        if (this.courseRepository == null)
        //        {
        //            this.courseRepository = new GenericRepository<Course>(context);
        //        }
        //        return courseRepository;
        //    }
        //}

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
