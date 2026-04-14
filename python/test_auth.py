import os
import time
import unittest
import requests

BASE_URL = os.getenv('API_URL', 'http://localhost:5000')
REGISTER_URL = f'{BASE_URL}/auth/register'
LOGIN_URL = f'{BASE_URL}/auth/login'
ME_URL = f'{BASE_URL}/auth/me'
REFRESH_URL = f'{BASE_URL}/auth/refresh'
LOGOUT_URL = f'{BASE_URL}/auth/logout'


class AuthApiTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ts = int(time.time())
        cls.username = f'testuser_{ts}'
        cls.email = f'{cls.username}@example.com'
        cls.password = 'Secret123!'
        cls.session = requests.Session()

    def test_01_register(self):
        response = requests.post(
            REGISTER_URL,
            json={
                'username': self.username,
                'email': self.email,
                'password': self.password,
            },
        )
        self.assertEqual(response.status_code, 201)
        data = response.json()
        self.assertIn('access_token', data)
        self.assertIn('user', data)
        self.assertEqual(data['user']['username'], self.username)
        self.assertEqual(data['user']['email'], self.email)
        self.assertEqual(data['user']['role'], 'user')

    def test_02_login(self):
        response = requests.post(
            LOGIN_URL,
            json={'username': self.username, 'password': self.password},
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn('access_token', data)
        self.__class__.access_token = data['access_token']

    def test_03_me(self):
        token = getattr(self.__class__, 'access_token', None)
        self.assertIsNotNone(token)

        response = requests.get(ME_URL, headers={'Authorization': f'Bearer {token}'})
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data['username'], self.username)
        self.assertEqual(data['email'], self.email)
        self.assertEqual(data['role'], 'user')

    def test_04_refresh(self):
        response = self.session.post(
            LOGIN_URL,
            json={'username': self.username, 'password': self.password},
        )
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.cookies.get('refresh_token'))

        refresh_response = self.session.post(REFRESH_URL)
        self.assertEqual(refresh_response.status_code, 200)
        refresh_data = refresh_response.json()
        self.assertIn('access_token', refresh_data)

    def test_05_logout(self):
        response = requests.post(
            LOGIN_URL,
            json={'username': self.username, 'password': self.password},
        )
        self.assertEqual(response.status_code, 200)
        token = response.json()['access_token']
        session = requests.Session()
        session.headers.update({'Authorization': f'Bearer {token}'})
        session.cookies.update(response.cookies.get_dict())

        logout_response = session.post(LOGOUT_URL)
        self.assertEqual(logout_response.status_code, 200)
        self.assertEqual(logout_response.json().get('status'), 'ok')

        refresh_response = session.post(REFRESH_URL)
        self.assertNotEqual(refresh_response.status_code, 200)


if __name__ == '__main__':
    unittest.main()
