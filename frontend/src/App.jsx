
import { useEffect, useState } from 'react'
import './App.css'

const API_URL = 'http://localhost:5056/api/tasks'

function App() {
  const [tasks, setTasks] = useState([])
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [editingId, setEditingId] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function loadTasks() {
    setLoading(true)
    setError('')

    try {
      const response = await fetch(API_URL)

      if (!response.ok) {
        throw new Error('Failed to load tasks.')
      }

      const data = await response.json()
      setTasks(data)
    } catch (err) {
      setError(
        'Could not connect to the API. Make sure the .NET backend is running.'
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadTasks()
  }, [])

  function resetForm() {
    setTitle('')
    setDescription('')
    setEditingId(null)
  }

  async function handleSubmit(event) {
    event.preventDefault()

    if (!title.trim()) {
      setError('Task title is required.')
      return
    }

    setSaving(true)
    setError('')

    const isEditing = editingId !== null

    const payload = {
      title: title.trim(),
      description: description.trim() || null,
      ...(isEditing && {
        isCompleted: tasks.find((task) => task.id === editingId)?.isCompleted ?? false,
      }),
    }

    try {
      const response = await fetch(
        isEditing ? `${API_URL}/${editingId}` : API_URL,
        {
          method: isEditing ? 'PUT' : 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
        }
      )

      if (!response.ok) {
        throw new Error(
          isEditing ? 'Failed to update task.' : 'Failed to create task.'
        )
      }

      resetForm()
      await loadTasks()
    } catch (err) {
      setError(err.message || 'Something went wrong.')
    } finally {
      setSaving(false)
    }
  }

  function handleEdit(task) {
    setEditingId(task.id)
    setTitle(task.title)
    setDescription(task.description ?? '')
    setError('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function handleToggle(task) {
    setError('')

    try {
      const response = await fetch(`${API_URL}/${task.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          title: task.title,
          description: task.description,
          isCompleted: !task.isCompleted,
        }),
      })

      if (!response.ok) {
        throw new Error('Failed to update task status.')
      }

      await loadTasks()
    } catch (err) {
      setError(err.message || 'Could not update task status.')
    }
  }

  async function handleDelete(id) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this task?'
    )

    if (!confirmed) return

    setError('')

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error('Failed to delete task.')
      }

      if (editingId === id) {
        resetForm()
      }

      await loadTasks()
    } catch (err) {
      setError(err.message || 'Could not delete task.')
    }
  }

  const completedCount = tasks.filter((task) => task.isCompleted).length

  return (
    <main className="app">
      <header className="topbar">
        <a className="brand" href="/">
          <span className="brand-icon">✓</span>
          TaskFlow
        </a>
        <span className="tech-label">ASP.NET Core + React</span>
      </header>

      <section className="dashboard">
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
            <strong>{tasks.length - completedCount}</strong>
          </article>
        </div>

        <section className="panel form-panel">
          <div className="panel-heading">
            <div>
              <h2>{editingId ? 'Edit task' : 'Create a task'}</h2>
              <p>
                {editingId
                  ? 'Update the details below.'
                  : 'Add something you want to accomplish.'}
              </p>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            <label htmlFor="title">Task title</label>
            <input
              id="title"
              type="text"
              placeholder="e.g. Complete API integration"
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              maxLength={200}
              required
            />

            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              placeholder="Add some details (optional)"
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              rows={3}
            />

            <div className="form-actions">
              {editingId && (
                <button
                  className="button secondary"
                  type="button"
                  onClick={resetForm}
                >
                  Cancel
                </button>
              )}
              <button className="button primary" type="submit" disabled={saving}>
                {saving
                  ? 'Saving...'
                  : editingId
                    ? 'Save changes'
                    : '+ Add task'}
              </button>
            </div>
          </form>
        </section>

        {error && (
          <div className="error-message" role="alert">
            {error}
          </div>
        )}

        <section className="tasks-section">
          <div className="tasks-heading">
            <div>
              <h2>Your tasks</h2>
              <p>Manage all your tasks in one place.</p>
            </div>
            <span className="task-count">{tasks.length} tasks</span>
          </div>

          {loading ? (
            <div className="empty-state">Loading tasks...</div>
          ) : tasks.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">☑</div>
              <h3>No tasks yet</h3>
              <p>Create your first task using the form above.</p>
            </div>
          ) : (
            <div className="task-list">
              {tasks.map((task) => (
                <article
                  className={`task-card ${task.isCompleted ? 'completed' : ''}`}
                  key={task.id}
                >
                  <button
                    className={`check-button ${task.isCompleted ? 'checked' : ''}`}
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
                        className={`status ${task.isCompleted ? 'status-done' : 'status-pending'}`}
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
                    <button
                      className="action-button"
                      type="button"
                      onClick={() => handleEdit(task)}
                    >
                      Edit
                    </button>
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
      </section>

      <footer className="footer">
        Built with React, ASP.NET Core, and PostgreSQL
      </footer>
    </main>
  )
}

export default App
