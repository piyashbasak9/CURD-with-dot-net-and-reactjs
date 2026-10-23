
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createTask } from '../services/taskService'

export default function CreateTask() {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const navigate = useNavigate()

  async function handleSubmit(event) {
    event.preventDefault()

    if (!title.trim()) {
      setError('Task title is required.')
      return
    }

    try {
      setSaving(true)
      setError('')

      await createTask({
        title: title.trim(),
        description: description.trim() || null,
      })

      navigate('/tasks')
    } catch (err) {
      setError(err.message || 'Failed to create task.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="form-panel panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">YOUR WORKSPACE</p>
          <h1>Create a task</h1>
          <p>Add something you want to accomplish.</p>
        </div>
      </div>

      {error && (
        <p className="error-message" role="alert">
          {error}
        </p>
      )}

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
          rows={4}
        />

        <div className="form-actions">
          <Link className="button secondary" to="/tasks">
            Cancel
          </Link>

          <button
            className="button primary"
            type="submit"
            disabled={saving}
          >
            {saving ? 'Creating...' : 'Create task'}
          </button>
        </div>
      </form>
    </section>
  )
}
