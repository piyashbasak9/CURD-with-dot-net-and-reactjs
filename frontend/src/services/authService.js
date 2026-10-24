const API_URL = 'http://localhost:5056/api/auth'

async function handleResponse(response) {
  const data = response.status === 204 ? null : await response.json().catch(() => null)

  if (!response.ok) {
    const validationMessages = data?.errors
      ? Object.values(data.errors).flat().join(' ')
      : ''

    throw new Error(
      data?.message || validationMessages || `Request failed: ${response.status}`
    )
  }

  return data
}

async function post(path, body) {
  const response = await fetch(`${API_URL}/${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  return handleResponse(response)
}

export function register(details) {
  return post('register', details)
}

export function verifyEmail(details) {
  return post('verify-email', details)
}

export function login(credentials) {
  return post('login', credentials)
}
