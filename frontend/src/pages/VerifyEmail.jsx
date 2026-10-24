import { useState } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { verifyEmail } from '../services/authService'

export default function VerifyEmail() {
  const [searchParams] = useSearchParams()
  const [email, setEmail] = useState(searchParams.get('email') || '')
  const [code, setCode] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const location = useLocation()
  const navigate = useNavigate()

  async function handleSubmit(event) {
    event.preventDefault()

    try {
      setSubmitting(true)
      setError('')
      await verifyEmail({ email: email.trim(), code: code.trim() })
      navigate('/login', {
        replace: true,
        state: { message: 'Email verified. You can now sign in.' },
      })
    } catch (err) {
      setError(err.message || 'Failed to verify your email.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="form-panel panel auth-panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">EMAIL VERIFICATION</p>
          <h1>Verify your email</h1>
          <p>
            {location.state?.message || 'Enter the 6-digit code sent to your email.'}
          </p>
        </div>
      </div>

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

        <label htmlFor="code">Verification code</label>
        <input
          id="code"
          type="text"
          inputMode="numeric"
          autoComplete="one-time-code"
          pattern="[0-9]{6}"
          maxLength={6}
          value={code}
          onChange={(event) => setCode(event.target.value)}
          required
        />

        <button className="button primary" type="submit" disabled={submitting}>
          {submitting ? 'Verifying...' : 'Verify email'}
        </button>
      </form>

      <p className="auth-switch">
        Already verified? <Link to="/login">Sign in</Link>
      </p>
    </section>
  )
}
