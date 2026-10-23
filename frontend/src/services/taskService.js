const API_URL = 'http://localhost:5056/api/tasks'

async function handleResponse(response) {
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`)
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export async function getTasks() {
  const response = await fetch(API_URL)
  return handleResponse(response)
}

export async function getTaskById(id) {
  const response = await fetch(`${API_URL}/${id}`)
  return handleResponse(response)
}

export async function createTask(task) {
  const response = await fetch(API_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(task),
  })

  return handleResponse(response)
}

export async function updateTask(id, task) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(task),
  })

  return handleResponse(response)
}

export async function deleteTask(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: 'DELETE',
  })

  return handleResponse(response)
}