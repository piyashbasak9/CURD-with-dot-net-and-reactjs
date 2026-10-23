import { Outlet } from 'react-router-dom'
import Navbar from '../components/Navbar'

function MainLayout() {
  return (
    <main className="app">
      <Navbar />

      <div className="dashboard">
        <Outlet />
      </div>

      <footer className="footer">
        Built with React, ASP.NET Core, and PostgreSQL
      </footer>
    </main>
  )
}

export default MainLayout