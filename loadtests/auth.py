from locust import HttpLocust, TaskSet, task, between

import random
import string
import base64

letters = string.ascii_lowercase


class UserBehavior(TaskSet):
    req = 0
    resp = 0
    def on_start(self):
        self.token = None
        self.name = ''.join(random.choice(letters) for _ in range(10))
        self.psw = ''.join(random.choice(letters) for _ in range(10))

        #print('name:  ' + self.name)
        #print('psw:  ' + self.psw)

        with self.client.post("registration", headers={'Authorization': 'Basic ' + self._get_name_psw_encoded(self.name, self.psw)}) as response:
            self.token = str(response.content)
            #print(response.url)
            #print(str(response.reason) + '   ' + str(response.status_code) + '   ' + str(response.text))
            #print('/n/n/n/n ' + self.token + ' /n/n/n/n/n/n')

    def on_stop(self):
        self.client.delete("registration", headers={'Authorization': 'Basic ' + self._get_name_psw_encoded(self.name, self.psw)})

    @task(1)
    def auth(self):
        if self.token:
            #UserBehavior.req += 1
            #print(str(UserBehavior.req) + "req")
            self.client.get('auth', headers={'Authorization': 'Bearer ' + self.token})
            #UserBehavior.resp += 1
            #print(str(UserBehavior.resp) + 'resp')

    def _get_name_psw_encoded(self, name, psw):
        ret_val = base64.b64encode(name + ':' + psw)

        return ret_val


class WebsiteUser(HttpLocust):
    task_set = UserBehavior
    wait_time = between(0.500, 0.600)
