import { NavLink } from 'react-router-dom'

function Navbar() {
  return (
    <header className="topbar">
      <NavLink className="brand" to="/">
        <span className="brand-icon">✓</span>
        TaskFlow
      </NavLink>

      <nav className="nav-links">
        <NavLink to="/">Dashboard</NavLink>
        <NavLink to="/tasks">All Tasks</NavLink>
      </nav>

      <span className="tech-label">ASP.NET Core + React</span>
    </header>
  )
}

export default Navbar