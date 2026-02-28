<template>
  <div class="home">
    <div class="home-container">
      <h1>Finanzas Personales <span class="badge">MVP 0</span></h1>
      <p class="subtitle">Sistema bimonetario USD/ARS - Navegación de pantallas</p>
      
      <div class="nav-grid">
        <router-link to="/dashboard" class="nav-card">
          <div class="nav-card-title">📊 Dashboard</div>
          <div class="nav-card-desc">Vista general mensual con KPIs, costo ARS/USD, patrimonio y ahorro promedio</div>
        </router-link>
        
        <router-link to="/gastos-fijos" class="nav-card">
          <div class="nav-card-title">💳 Gastos Fijos</div>
          <div class="nav-card-desc">Trackeo de gastos recurrentes por banco y cash con estados de pago</div>
        </router-link>
        
        <router-link to="/tarjetas" class="nav-card">
          <div class="nav-card-title">💳 Tarjetas</div>
          <div class="nav-card-desc">Gestión de compras variables, cuotas y deuda pendiente futura</div>
        </router-link>
        
        <router-link to="/caja-usd" class="nav-card">
          <div class="nav-card-title">💵 Caja USD</div>
          <div class="nav-card-desc">Buckets de USD líquido por cuenta con movimientos reales y forecast</div>
        </router-link>
        
        <a href="#" class="nav-card disabled">
          <div class="nav-card-title">📈 Inversiones</div>
          <div class="nav-card-desc">Seguimiento de cuentas de inversión y progreso vs objetivo anual</div>
        </a>
        
        <router-link to="/configuracion" class="nav-card">
          <div class="nav-card-title">⚙️ Configuración</div>
          <div class="nav-card-desc">Gestión de tipo de cambio, cuentas, tarjetas, categorías y objetivos</div>
        </router-link>
      </div>
      
      <div class="footer">
        MVP 0 - Finanzas bimonetarias Argentina
      </div>
    </div>
  </div>
</template>

<style scoped>
.home {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.home-container {
  max-width: 900px;
  width: 100%;
  background: white;
  border-radius: 16px;
  padding: 48px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}

.home h1 {
  font-size: 36px;
  font-weight: 700;
  margin-bottom: 12px;
  color: #2c3e50;
}

.subtitle {
  font-size: 16px;
  color: #7f8c8d;
  margin-bottom: 40px;
}

.nav-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}

.nav-card {
  background: #f8f9fa;
  padding: 24px;
  border-radius: 12px;
  text-decoration: none;
  color: #2c3e50;
  transition: all 0.3s ease;
  border: 2px solid transparent;
  cursor: pointer;
}

.nav-card:not(.disabled):hover {
  background: white;
  border-color: #3498db;
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(52, 152, 219, 0.2);
}

.nav-card.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.nav-card-title {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 8px;
  color: #2c3e50;
}

.nav-card-desc {
  font-size: 14px;
  color: #7f8c8d;
  line-height: 1.5;
}

.footer {
  text-align: center;
  padding-top: 24px;
  border-top: 1px solid #ecf0f1;
  color: #95a5a6;
  font-size: 14px;
}

.badge {
  display: inline-block;
  background: #3498db;
  color: white;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  margin-left: 8px;
}
</style>
