import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register } from '../services/authService'

export default function Register() {
  const [details, setDetails] = useState({
    name: '',
    email: '',
    phoneNumber: '',
    address: '',
    password: '',
  })
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const navigate = useNavigate()

  function updateField(event) {
    const { name, value } = event.target
    setDetails((current) => ({ ...current, [name]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()

    try {
      setSubmitting(true)
      setError('')
      await register({
        ...details,
        name: details.name.trim(),
        email: details.email.trim(),
        phoneNumber: details.phoneNumber.trim(),
        address: details.address.trim() || null,
      })
      navigate(`/verify-email?email=${encodeURIComponent(details.email.trim())}`, {
        state: { message: 'Account created. Check your email for the verification code.' },
      })
    } catch (err) {
      setError(err.message || 'Failed to create your account.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="form-panel panel auth-panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">GET STARTED</p>
          <h1>Create your account</h1>
          <p>Verify your email after registration to sign in.</p>
        </div>
      </div>

      {error && <p className="error-message" role="alert">{error}</p>}

      <form onSubmit={handleSubmit}>
        <label htmlFor="name">Name</label>
        <input
          id="name"
          name="name"
          type="text"
          autoComplete="name"
          minLength={2}
          maxLength={100}
          value={details.name}
          onChange={updateField}
          required
        />

        <label htmlFor="email">Email</label>
        <input
          id="email"
          name="email"
          type="email"
          autoComplete="email"
          maxLength={256}
          value={details.email}
          onChange={updateField}
          required
        />

        <label htmlFor="phoneNumber">Phone number</label>
        <input
          id="phoneNumber"
          name="phoneNumber"
          type="tel"
          autoComplete="tel"
          maxLength={30}
          value={details.phoneNumber}
          onChange={updateField}
          required
        />

        <label htmlFor="address">Address (optional)</label>
        <textarea
          id="address"
          name="address"
          autoComplete="street-address"
          maxLength={500}
          rows={2}
          value={details.address}
          onChange={updateField}
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          name="password"
          type="password"
          autoComplete="new-password"
          pattern="(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}"
          minLength={8}
          maxLength={100}
          value={details.password}
          onChange={updateField}
          required
        />
        <p className="field-hint">
          Use at least 8 characters with an uppercase letter, a lowercase letter, and a number.
        </p>

        <button className="button primary" type="submit" disabled={submitting}>
          {submitting ? 'Creating account...' : 'Create account'}
        </button>
      </form>

      <p className="auth-switch">
        Already registered? <Link to="/login">Sign in</Link>
      </p>
    </section>
  )
}
