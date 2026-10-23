
import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <section className="not-found">
      <p className="eyebrow">ERROR 404</p>
      <h1>Page not found</h1>
      <p>
        Sorry, the page you are looking for does not exist
        or may have been moved.
      </p>

      <div className="form-actions">
        <Link className="button primary" to="/">
          Go to Dashboard
        </Link>

        <Link className="button secondary" to="/tasks">
          View all tasks
        </Link>
      </div>
    </section>
  )
}
