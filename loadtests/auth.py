from locust import HttpLocust, TaskSet, task, between

import random
import string
import base64
import logging

letters = string.ascii_lowercase


class UserBehavior(TaskSet):
    req = 0
    resp = 0
    def on_start(self):
        self.token = None
        self.name = ''.join(random.choice(letters) for _ in range(10))
        self.psw = ''.join(random.choice(letters) for _ in range(10))

        with self.client.post("api/authentication", json = '{}', headers={'Content-Type': 'application/json', 'Authorization': 'Basic ' + self._get_name_psw_encoded(self.name, self.psw)}) as response:
            if response.ok:
                self.token = response.json()['token']
            else:
                logging.error(f'ERROR:   {response.content},   {response.status_code}')

    def on_stop(self):
        self.client.delete("api/authentication", headers={'Authorization': 'Basic ' + self._get_name_psw_encoded(self.name, self.psw)})

    @task(1)
    def auth(self):
        if self.token:
            self.client.get('api/authorization/public', headers={'Authorization': 'Bearer ' + self.token})

    def _get_name_psw_encoded(self, name, psw):
        ret_val = base64.b64encode(bytes(name + ':' + psw, 'utf-8'))
        ret_val = ret_val.decode('utf-8')

        return ret_val


class WebsiteUser(HttpLocust):
    task_set = UserBehavior
    wait_time = between(0.500, 0.600)
