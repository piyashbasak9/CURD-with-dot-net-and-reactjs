const API_URL = 'http://localhost:5056/api/tasks'
const TOKEN_STORAGE_KEY = 'taskflow.accessToken'

function authorizationHeaders() {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY)

  return token ? { Authorization: `Bearer ${token}` } : {}
}

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
  const response = await fetch(API_URL, {
    headers: authorizationHeaders(),
  })
  return handleResponse(response)
}

export async function getTaskById(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    headers: authorizationHeaders(),
  })
  return handleResponse(response)
}

export async function createTask(task) {
  const response = await fetch(API_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...authorizationHeaders(),
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
      ...authorizationHeaders(),
    },
    body: JSON.stringify(task),
  })

  return handleResponse(response)
}

export async function deleteTask(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: 'DELETE',
    headers: authorizationHeaders(),
  })

  return handleResponse(response)
}