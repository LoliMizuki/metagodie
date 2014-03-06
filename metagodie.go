package main

import (
	"fmt"
	"io/ioutil"
	"path/filepath"
	// "io"
	// "io/ioutil"
	// "fmt"
	"os"
	// "os/exec"
)

func main() {
	if len(os.Args) < 2 {
		fmt.Println("usage: metagodie <dir>")
		return
	}
	targetDirectory := os.Args[1]
	recursiveLookUpInDirectoryAndRemoveMeta(targetDirectory)
}

func recursiveLookUpInDirectoryAndRemoveMeta(directory string) {
	fileInfos, err := ioutil.ReadDir(directory)
	if err != nil {
		fmt.Println("err", err)
		return
	}

	for _, fileInfo := range fileInfos {
		fullPath := directory + "/" + fileInfo.Name()

		if isDirectory(fullPath) {
			recursiveLookUpInDirectoryAndRemoveMeta(fullPath)
		} else {
			if isSuckUnityMetaFile(fullPath) {
				os.Remove(fullPath)
			}
		}
	}
}

func isDirectory(path string) bool {
	info, err := os.Stat(path)
	if err != nil {
		fmt.Println("err", err)
		return false
	}

	return info.IsDir()
}

func isSuckUnityMetaFile(path string) bool {
	fileName := filepath.Base(path)
	lengthOfName := len(fileName)

	if lengthOfName <= 4 {
		return false
	}

	if fileName[lengthOfName-4:] == "meta" {
		return true
	}

	return false
}

// sudo find <path> -name ".svn" -exec rm -r {} \; ... why this commad can not work?
