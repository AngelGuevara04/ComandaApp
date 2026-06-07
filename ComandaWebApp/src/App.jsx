import React, { useState, useEffect, useMemo } from 'react';
import { supabase } from './supabaseClient';
import { ShoppingCart, ChefHat, Plus, Minus, X, Check, Loader2 } from 'lucide-react';

function App() {
  const [platillos, setPlatillos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [cart, setCart] = useState([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [selectedPlatillo, setSelectedPlatillo] = useState(null);
  const [qty, setQty] = useState(1);
  const [notas, setNotas] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [orderSuccess, setOrderSuccess] = useState(false);
  const [businessName, setBusinessName] = useState('Menú Digital');

  // Parse URL params
  const searchParams = new URLSearchParams(window.location.search);
  const negocioId = searchParams.get('negocio');
  const numeroMesa = searchParams.get('mesa');

  useEffect(() => {
    if (negocioId) {
      fetchConfigAndPlatillos();
    } else {
      setLoading(false);
    }
  }, [negocioId]);

  const fetchConfigAndPlatillos = async () => {
    try {
      // Fetch config
      const { data: configData } = await supabase
        .from('configuracion_negocio')
        .select('nombre_restaurante')
        .eq('negocio_id', negocioId)
        .maybeSingle();
        
      if (configData && configData.nombre_restaurante) {
        setBusinessName(configData.nombre_restaurante);
      }

      // Fetch platillos
      const { data, error } = await supabase
        .from('platillos')
        .select('*')
        .eq('negocio_id', negocioId)
        .eq('disponible', true)
        .order('categoria', { ascending: true })
        .order('nombre', { ascending: true });

      if (error) throw error;
      setPlatillos(data || []);
    } catch (err) {
      console.error('Error fetching data:', err);
      alert('Error al cargar el menú.');
    } finally {
      setLoading(false);
    }
  };

  const categorias = useMemo(() => {
    const cats = new Set(platillos.map(p => p.categoria));
    return Array.from(cats);
  }, [platillos]);

  const openPlatilloModal = (platillo) => {
    setSelectedPlatillo(platillo);
    setQty(1);
    setNotas('');
  };

  const closePlatilloModal = () => {
    setSelectedPlatillo(null);
  };

  const addToCart = () => {
    if (!selectedPlatillo) return;
    
    setCart(prev => [...prev, {
      id: crypto.randomUUID(),
      platillo: selectedPlatillo,
      cantidad: qty,
      notas: notas
    }]);
    
    closePlatilloModal();
  };

  const removeFromCart = (id) => {
    setCart(prev => prev.filter(item => item.id !== id));
  };

  const cartTotal = useMemo(() => {
    return cart.reduce((total, item) => total + (item.platillo.precio * item.cantidad), 0);
  }, [cart]);

  const placeOrder = async () => {
    if (cart.length === 0) return;
    setIsSubmitting(true);

    try {
      // 1. Get or create active order for this table
      let ordenId = null;
      
      const { data: existingOrders, error: fetchError } = await supabase
        .from('ordenes')
        .select('id')
        .eq('numero_mesa', numeroMesa)
        .eq('negocio_id', negocioId)
        .eq('esta_pagada', false);

      if (fetchError) throw fetchError;

      if (existingOrders && existingOrders.length > 0) {
        ordenId = existingOrders[0].id;
      } else {
        ordenId = crypto.randomUUID();
        const { error: insertOrdenError } = await supabase
          .from('ordenes')
          .insert({
            id: ordenId,
            numero_mesa: numeroMesa,
            nombre_cliente: 'Cliente Web',
            fecha_creacion: new Date().toISOString(),
            esta_pagada: false,
            negocio_id: negocioId
          });
          
        if (insertOrdenError) throw insertOrdenError;
      }

      // 2. Insert details
      const detalles = cart.map(item => ({
        id: crypto.randomUUID(),
        orden_id: ordenId,
        nombre_platillo: item.platillo.nombre,
        cantidad: item.cantidad,
        precio_unitario: item.platillo.precio,
        notas: item.notas || '',
        estado: 'Pendiente',
        negocio_id: negocioId
      }));

      const { error: insertDetallesError } = await supabase
        .from('detalles_pedido')
        .insert(detalles);

      if (insertDetallesError) throw insertDetallesError;

      // Success
      setCart([]);
      setIsCartOpen(false);
      setOrderSuccess(true);
      setTimeout(() => setOrderSuccess(false), 5000);

    } catch (err) {
      console.error('Error placing order:', err);
      alert('Hubo un error al procesar tu orden. Por favor intenta de nuevo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!negocioId || !numeroMesa) {
    return (
      <div className="flex flex-col items-center justify-center" style={{ minHeight: '100vh', padding: '2rem', textAlign: 'center' }}>
        <ChefHat size={64} className="mb-4" style={{ color: 'var(--primary-color)' }} />
        <h2>Escanea el QR de tu mesa</h2>
        <p className="mt-4" style={{ color: 'var(--text-secondary)' }}>
          Necesitas escanear un código válido para ver el menú y pedir.
        </p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center" style={{ minHeight: '100vh' }}>
        <Loader2 size={48} className="animate-spin" style={{ color: 'var(--primary-color)' }} />
      </div>
    );
  }

  return (
    <>
      <header className="header flex justify-between items-center">
        <div className="flex items-center gap-2">
          <ChefHat size={28} style={{ color: 'var(--primary-color)' }} />
          <h1>{businessName}</h1>
        </div>
        <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
          Mesa <strong style={{ color: 'var(--text-primary)' }}>{numeroMesa}</strong>
        </div>
      </header>

      <main className="container flex-col gap-4">
        {orderSuccess && (
          <div className="card" style={{ backgroundColor: 'rgba(16, 185, 129, 0.1)', borderColor: 'var(--success-color)', marginBottom: '1rem' }}>
            <div className="flex items-center gap-2" style={{ color: 'var(--success-color)' }}>
              <Check size={24} />
              <strong style={{ fontSize: '1.1rem' }}>¡Orden enviada a cocina!</strong>
            </div>
            <p className="mt-4" style={{ color: 'var(--text-primary)' }}>Tu pedido ya está siendo preparado.</p>
          </div>
        )}

        {categorias.map(categoria => (
          <div key={categoria} className="mb-4">
            <h2 style={{ fontSize: '1.25rem', marginBottom: '1rem', color: 'var(--primary-color)' }}>{categoria}</h2>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1rem' }}>
              {platillos.filter(p => p.categoria === categoria).map(platillo => (
                <div key={platillo.id} className="card" onClick={() => openPlatilloModal(platillo)}>
                  {/* Assuming image loading if valid, else placeholder */}
                  {platillo.imagen_url && platillo.imagen_url !== 'dotnet_bot.svg' ? (
                    <img src={platillo.imagen_url} alt={platillo.nombre} className="dish-image" />
                  ) : (
                    <div className="dish-image flex items-center justify-center">
                      <ChefHat size={48} style={{ color: 'var(--text-secondary)', opacity: 0.5 }} />
                    </div>
                  )}
                  <div className="flex justify-between items-center">
                    <h3 style={{ fontSize: '1.1rem' }}>{platillo.nombre}</h3>
                    <strong style={{ color: 'var(--success-color)' }}>${platillo.precio.toFixed(2)}</strong>
                  </div>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', marginTop: '0.5rem', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                    {platillo.descripcion || 'Sin descripción'}
                  </p>
                </div>
              ))}
            </div>
          </div>
        ))}
      </main>

      {/* Cart Bottom Bar */}
      {cart.length > 0 && (
        <div className="bottom-bar">
          <div className="bottom-bar-content">
            <div className="flex flex-col">
              <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{cart.length} articulos</span>
              <strong style={{ fontSize: '1.25rem' }}>Total: ${cartTotal.toFixed(2)}</strong>
            </div>
            <button className="btn btn-primary" onClick={() => setIsCartOpen(true)}>
              <ShoppingCart size={20} />
              Ver Orden
            </button>
          </div>
        </div>
      )}

      {/* Item Modal */}
      {selectedPlatillo && (
        <div className="modal-overlay" onClick={closePlatilloModal}>
          <div className="modal-content flex-col gap-4" onClick={e => e.stopPropagation()}>
            <div className="flex justify-between items-center mb-4">
              <h2 style={{ fontSize: '1.5rem' }}>{selectedPlatillo.nombre}</h2>
              <button className="btn-icon btn-outline" onClick={closePlatilloModal}><X size={24} /></button>
            </div>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>{selectedPlatillo.descripcion}</p>
            
            <div className="flex items-center justify-between mb-4">
              <span style={{ fontWeight: 600, fontSize: '1.25rem' }}>${(selectedPlatillo.precio * qty).toFixed(2)}</span>
              <div className="qty-control">
                <button className="qty-btn" onClick={() => setQty(Math.max(1, qty - 1))}><Minus size={20}/></button>
                <span className="qty-value">{qty}</span>
                <button className="qty-btn" onClick={() => setQty(qty + 1)}><Plus size={20}/></button>
              </div>
            </div>

            <div className="mb-4">
              <label style={{ display: 'block', marginBottom: '0.5rem', fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Notas especiales</label>
              <textarea 
                className="input" 
                rows="3" 
                placeholder="Ej. Sin cebolla, extra aderezo..."
                value={notas}
                onChange={e => setNotas(e.target.value)}
              />
            </div>

            <button className="btn btn-primary" style={{ width: '100%', padding: '1rem' }} onClick={addToCart}>
              Agregar al Carrito
            </button>
          </div>
        </div>
      )}

      {/* Cart Modal */}
      {isCartOpen && (
        <div className="modal-overlay" onClick={() => setIsCartOpen(false)}>
          <div className="modal-content flex-col gap-4" style={{ height: '90vh', display: 'flex' }} onClick={e => e.stopPropagation()}>
            <div className="flex justify-between items-center mb-4">
              <h2 style={{ fontSize: '1.5rem' }}>Tu Pedido</h2>
              <button className="btn-icon btn-outline" onClick={() => setIsCartOpen(false)}><X size={24} /></button>
            </div>

            <div style={{ flex: 1, overflowY: 'auto' }} className="flex-col gap-4">
              {cart.map(item => (
                <div key={item.id} className="card flex items-center justify-between" style={{ padding: '0.75rem' }}>
                  <div className="flex items-center gap-4">
                    <div className="badge">{item.cantidad}</div>
                    <div className="flex-col">
                      <strong>{item.platillo.nombre}</strong>
                      {item.notas && <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Nota: {item.notas}</span>}
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <strong>${(item.platillo.precio * item.cantidad).toFixed(2)}</strong>
                    <button className="btn-icon" style={{ color: 'var(--danger-color)' }} onClick={() => removeFromCart(item.id)}>
                      <X size={20} />
                    </button>
                  </div>
                </div>
              ))}
            </div>

            <div style={{ marginTop: 'auto', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
              <div className="flex justify-between items-center mb-4">
                <span style={{ fontSize: '1.25rem', color: 'var(--text-secondary)' }}>Total a Pagar</span>
                <strong style={{ fontSize: '1.5rem' }}>${cartTotal.toFixed(2)}</strong>
              </div>
              <button 
                className="btn btn-primary" 
                style={{ width: '100%', padding: '1rem', fontSize: '1.1rem' }} 
                onClick={placeOrder}
                disabled={isSubmitting}
              >
                {isSubmitting ? <Loader2 className="animate-spin" /> : 'Confirmar Pedido'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default App;
