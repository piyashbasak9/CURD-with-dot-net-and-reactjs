
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  getTaskById,
  updateTask,
} from '../services/taskService'

export default function EditTask() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [isCompleted, setIsCompleted] = useState(false)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadTask() {
      try {
        setError('')

        const task = await getTaskById(id)

        setTitle(task.title)
        setDescription(task.description ?? '')
        setIsCompleted(task.isCompleted)
      } catch (err) {
        setError(err.message || 'Failed to load task.')
      } finally {
        setLoading(false)
      }
    }

    loadTask()
  }, [id])

  async function handleSubmit(event) {
    event.preventDefault()

    if (!title.trim()) {
      setError('Task title is required.')
      return
    }

    try {
      setSaving(true)
      setError('')

      await updateTask(id, {
        title: title.trim(),
        description: description.trim() || null,
        isCompleted,
      })

      navigate('/tasks')
    } catch (err) {
      setError(err.message || 'Failed to update task.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return <p>Loading task...</p>
  }

  return (
    <section className="form-panel panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">YOUR WORKSPACE</p>
          <h1>Edit task</h1>
          <p>Update the details of your task.</p>
        </div>
      </div>

      {error && (
        <p className="error-message" role="alert">
          {error}
        </p>
      )}

      {!error || title ? (
        <form onSubmit={handleSubmit}>
          <label htmlFor="title">Task title</label>
          <input
            id="title"
            type="text"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            maxLength={200}
            required
          />

          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            rows={4}
          />

          <label className="edit-status-label">
            <input
              type="checkbox"
              checked={isCompleted}
              onChange={(event) => setIsCompleted(event.target.checked)}
            />
            Mark as completed
          </label>

          <div className="form-actions">
            <Link className="button secondary" to="/tasks">
              Cancel
            </Link>

            <button
              className="button primary"
              type="submit"
              disabled={saving}
            >
              {saving ? 'Saving...' : 'Save changes'}
            </button>
          </div>
        </form>
      ) : null}
    </section>
  )
}
