import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import useAuth from '../hooks/useAuth'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const { login } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const returnTo = location.state?.from?.pathname || '/'

  async function handleSubmit(event) {
    event.preventDefault()

    try {
      setSubmitting(true)
      setError('')
      await login({ email: email.trim(), password })
      navigate(returnTo, {
        replace: true,
        state: location.state?.message
          ? { message: location.state.message }
          : undefined,
      })
    } catch (err) {
      setError(err.message || 'Failed to sign in.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="form-panel panel auth-panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">WELCOME BACK</p>
          <h1>Sign in to TaskFlow</h1>
          <p>Use your verified email address to continue.</p>
        </div>
      </div>

      {location.state?.message && (
        <p className="success-message" role="status">
          {location.state.message}
        </p>
      )}

      {error && <p className="error-message" role="alert">{error}</p>}

      <form onSubmit={handleSubmit}>
        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />

        <button className="button primary" type="submit" disabled={submitting}>
          {submitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      <p className="auth-switch">
        Don&apos;t have an account? <Link to="/register">Create one</Link>
      </p>
    </section>
  )
}
