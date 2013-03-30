import socket
import qrcode
import Tkinter as tk
import thread
import time
from PIL import ImageTk

### Server Data
HOST = "" ## me
PORT = 6666 ## could also be random
ADDR = (HOST,PORT) ## address is tuple
BUFSIZE = 4096 ##buffersize - whatever


def startDatServer(whatever):
    ## setup tcp server
    daServSock = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    daServSock.bind(ADDR)
    daServSock.listen(5)
    print('server started')
    client,addr = daServSock.accept()
    print("Client connected: ")
    print(str(addr))
    print(str(client.recv(4096)))
    client.send("HEY")
    time.sleep(1)
    print("send connected")
    client.send("connected;0")
    tst = ""
    while 1:
        print("wariting for message")
        tst = str(client.recv(4096))
        print(tst)
        if( "Ready" in tst ):
            break
    print("send start")
    client.send("start;0")
    time.sleep(2)
    print("send sound")
    client.send("sound;walk")
    while 1:
        print(str(client.recv(4096)))


thread.start_new_thread(startDatServer,("whatever",))

## creat qrcode
img = qrcode.make(socket.gethostbyname(socket.gethostname())+";"+str(PORT)+";"+"1")


## show it
root = tk.Tk()
root.title('background image')

image1 = ImageTk.PhotoImage(img)

w = image1.width()
h = image1.height()
x = 0
y = 0

root.geometry("%dx%d+%d+%d" % (w, h, x, y))

panel1 = tk.Label(root,image=image1)
panel1.pack(side='top',fill='both',expand='yes')
panel1.image = image1

root.mainloop()
#root.show()


