
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getTasks,
  updateTask,
  deleteTask,
} from '../services/taskService'

export default function Tasks() {
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('all')

  async function loadTasks() {
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

  useEffect(() => {
    loadTasks()
  }, [])

  async function handleToggle(task) {
    try {
      setError('')

      await updateTask(task.id, {
        title: task.title,
        description: task.description,
        isCompleted: !task.isCompleted,
      })

      await loadTasks()
    } catch (err) {
      setError(err.message || 'Failed to update task.')
    }
  }

  async function handleDelete(id) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this task?'
    )

    if (!confirmed) return

    try {
      setError('')
      await deleteTask(id)
      await loadTasks()
    } catch (err) {
      setError(err.message || 'Failed to delete task.')
    }
  }

  const filteredTasks = tasks.filter((task) => {
    if (filter === 'completed') return task.isCompleted
    if (filter === 'pending') return !task.isCompleted
    return true
  })

  return (
    <section>
      <div className="page-heading">
        <p className="eyebrow">YOUR WORKSPACE</p>
        <h1>All Tasks</h1>
        <p className="subtitle">
          View and manage all your tasks in one place.
        </p>
      </div>

      <div className="tasks-heading">
        <div>
          <h2>Tasks ({filteredTasks.length})</h2>
          <p>Filter your tasks by status.</p>
        </div>

        <Link className="button primary" to="/tasks/new">
          + Create task
        </Link>
      </div>

      <div className="form-actions">
        <button
          className="button secondary"
          onClick={() => setFilter('all')}
        >
          All
        </button>

        <button
          className="button secondary"
          onClick={() => setFilter('pending')}
        >
          In progress
        </button>

        <button
          className="button secondary"
          onClick={() => setFilter('completed')}
        >
          Completed
        </button>
      </div>

      {error && (
        <p className="error-message" role="alert">
          {error}
        </p>
      )}

      {loading ? (
        <p className="empty-state">Loading tasks...</p>
      ) : filteredTasks.length === 0 ? (
        <div className="empty-state">
          <h3>No tasks found</h3>
          <p>Try another filter or create a new task.</p>
        </div>
      ) : (
        <div className="task-list">
          {filteredTasks.map((task) => (
            <article
              className={`task-card ${task.isCompleted ? 'completed' : ''}`}
              key={task.id}
            >
              <button
                className={`check-button ${
                  task.isCompleted ? 'checked' : ''
                }`}
                type="button"
                aria-label={
                  task.isCompleted
                    ? 'Mark task incomplete'
                    : 'Mark task complete'
                }
                onClick={() => handleToggle(task)}
              >
                {task.isCompleted ? '✓' : ''}
              </button>

              <div className="task-content">
                <h3>{task.title}</h3>

                {task.description && <p>{task.description}</p>}

                <div className="task-meta">
                  <span
                    className={`status ${
                      task.isCompleted
                        ? 'status-done'
                        : 'status-pending'
                    }`}
                  >
                    {task.isCompleted ? 'Completed' : 'In progress'}
                  </span>

                  {task.createdAtUtc && (
                    <span>
                      {new Date(task.createdAtUtc).toLocaleDateString()}
                    </span>
                  )}
                </div>
              </div>

              <div className="task-actions">
                <Link
                  className="action-button"
                  to={`/tasks/${task.id}/edit`}
                >
                  Edit
                </Link>

                <button
                  className="action-button delete-button"
                  type="button"
                  onClick={() => handleDelete(task.id)}
                >
                  Delete
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}
