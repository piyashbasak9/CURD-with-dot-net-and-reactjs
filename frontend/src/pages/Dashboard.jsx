import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getTasks } from '../services/taskService'

export default function Dashboard() {
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadDashboard() {
      try {
        setError('')
        const data = await getTasks()
        setTasks(data)
      } catch (err) {
        setError(err.message || 'Failed to load tasks.')
      } finally {
        setLoading(false)
      }
    }

    loadDashboard()
  }, [])

  const completedCount = tasks.filter(
    (task) => task.isCompleted
  ).length

  const remainingCount = tasks.length - completedCount

  return (
    <section>
      <div className="page-heading">
        <p className="eyebrow">YOUR WORKSPACE</p>
        <h1>Task Management</h1>
        <p className="subtitle">
          Organize your work, track progress, and get things done.
        </p>
      </div>

      <div className="stats">
        <article className="stat-card">
          <span className="stat-label">Total tasks</span>
          <strong>{tasks.length}</strong>
        </article>

        <article className="stat-card">
          <span className="stat-label">Completed</span>
          <strong>{completedCount}</strong>
        </article>

        <article className="stat-card">
          <span className="stat-label">Remaining</span>
          <strong>{remainingCount}</strong>
        </article>
      </div>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Recent tasks</h2>
            <p>Your tasks from the backend.</p>
          </div>

          <Link to="/tasks">View all</Link>
        </div>

        {loading ? (
          <p>Loading tasks...</p>
        ) : error ? (
          <p role="alert">{error}</p>
        ) : tasks.length === 0 ? (
          <p>No tasks yet. Create your first task!</p>
        ) : (
          <div className="task-list">
            {tasks.slice(0, 5).map((task) => (
              <article className="task-card" key={task.id}>
                <div className="task-content">
                  <h3>{task.title}</h3>

                  {task.description && <p>{task.description}</p>}

                  <span>
                    {task.isCompleted ? 'Completed' : 'In progress'}
                  </span>
                </div>
              </article>
            ))}
          </div>
        )}

        <div className="form-actions">
          <Link className="button primary" to="/tasks/new">
            + Create task
          </Link>
        </div>
      </section>
    </section>
  )
}