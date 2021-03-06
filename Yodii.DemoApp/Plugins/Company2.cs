#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\Company2.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : MonoWindowPlugin, IBusiness
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _delivery;
        ObservableCollection<ProductCompany2> _products;
        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> _orders;

        public Company2( IMarketPlaceService marketPlace, IDeliveryService delivery )
            : base( true )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
            _products = new ObservableCollection<ProductCompany2>();
            _orders = new ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>>();
        }

        protected override Window CreateWindow()
        {
            Window = new Company2View()
            {
                DataContext = this
            };
            return Window;
        }

        public class ProductCompany2 : Yodii.DemoApp.MarketPlace.Product
        {
            public ProductCompany2( string name, ProductCategory category, int price, DateTime creationDate, Company2 company )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
                CreationDate = creationDate;
                Company = company;
            }
        }

        internal void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price > 0 );

            ProductCompany2 p = new ProductCompany2( name, category, price, DateTime.Now, this );
            _marketPlace.AddNewProduct( p );
            _products.Add( p );
        }

        public bool NewOrder( IClientInfo clientInfo, MarketPlace.Product product )
        {
            Tuple<IClientInfo, MarketPlace.Product> order = new Tuple<IClientInfo, MarketPlace.Product>( clientInfo, product );
            if( _orders.Contains( order ) ) return false;
            _orders.Add( order );
            RaisePropertyChanged( "newOrder" );
            HandleOrders();
            return true;
        }

        public void HandleOrders()
        {
            if( _orders.Count >= 3 )
            {
                foreach( Tuple<IClientInfo, MarketPlace.Product> order in _orders )
                {
                    _delivery.Deliver( order );
                }
                _orders.Clear();
            }
        }

        public ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> Orders { get { return _orders; } }

        public ObservableCollection<ProductCompany2> Products { get { return _products; } }
    }
}
