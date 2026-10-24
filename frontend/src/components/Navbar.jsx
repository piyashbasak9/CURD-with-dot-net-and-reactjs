import { NavLink } from 'react-router-dom'
import useAuth from '../hooks/useAuth'

function Navbar() {
  const { isAuthenticated, logout } = useAuth()

  return (
    <header className="topbar">
      <NavLink className="brand" to="/">
        <span className="brand-icon">✓</span>
        TaskFlow
      </NavLink>

      <nav className="nav-links">
        {isAuthenticated ? (
          <>
            <NavLink to="/">Dashboard</NavLink>
            <NavLink to="/tasks">All Tasks</NavLink>
            <button className="nav-action" type="button" onClick={logout}>
              Sign out
            </button>
          </>
        ) : (
          <>
            <NavLink to="/login">Sign in</NavLink>
            <NavLink to="/register">Create account</NavLink>
          </>
        )}
      </nav>

      <span className="tech-label">ASP.NET Core + React</span>
    </header>
  )
}

export default Navbar