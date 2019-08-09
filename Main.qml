import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.3
import LoopMachineOsc 1.0

ApplicationWindow {
  id: window
  width: 400
  height: 500
  visible: true
  title: "TestWindow"

  Column {

    Row {
      Repeater {
        id: repeater
        Column {
          Text { text: modelData.title }
          Text { text: "LED"; color: modelData.ledColor }
          Button {
            text: "X"
            width: 20
            onClicked: {
              vm.press(modelData.buttonType)
            }
          }
        }
      }
    }
  }

  MainViewModel {
    id: vm
    Component.onCompleted: {
      repeater.model = Net.toListModel(vm.buttons)
    }
  }
}
